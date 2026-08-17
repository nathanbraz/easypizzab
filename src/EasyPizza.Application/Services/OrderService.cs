using EasyPizza.Application.Interfaces.Repositories;
using EasyPizza.Application.Interfaces.Services;
using EasyPizza.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EasyPizza.Application.Services;

public class OrderService(
    IOrderRepository repository,
    IStoreSettingsRepository settingsRepository,
    IPaymentTypeRepository paymentTypeRepository,
    ICouponRepository couponRepository,
    ICustomerRepository customerRepository,
    IWhatsappSender whatsappSender,
    ICatalogRepository catalogRepository,
    IPaymentGatewayService paymentGatewayService,
    ILogger<OrderService> logger) : IOrderService
{
    public async Task<Order> CreateOrderAsync(Guid customerId, Guid? customerAddressId, OrderType type, Guid paymentTypeId, List<OrderItemInput> items, string? couponCode = null, decimal? changeFor = null)
    {
        // --- 0. Recalcular preços a partir do catálogo ---
        // Nunca confiamos em UnitPrice/Price vindos do cliente: o preço final é sempre
        // recomputado aqui a partir do produto e das opções (próprias e compartilhadas da
        // categoria) que ele realmente oferece hoje. Isso evita que um payload manipulado
        // finalize um pedido com preço menor que o real.
        var pricedItems = new List<PricedOrderItem>();
        decimal subTotal = 0;

        foreach (var item in items)
        {
            var product = await catalogRepository.GetProductByIdAsync(item.ProductId)
                ?? throw new InvalidOperationException("Produto não encontrado.");

            if (!product.IsAvailable)
                throw new InvalidOperationException($"O produto \"{product.Name}\" não está disponível no momento.");

            var validOptions = (await catalogRepository.GetProductOptionsAsync(item.ProductId))
                .SelectMany(g => g.Options)
                .ToDictionary(o => o.Id);

            var recalculatedAddons = new List<OrderItemAddonInput>();
            decimal addonsTotal = 0;

            if (item.Addons != null)
            {
                foreach (var addon in item.Addons)
                {
                    if (addon.ProductOptionItemId == null || !validOptions.TryGetValue(addon.ProductOptionItemId.Value, out var catalogOption))
                        throw new InvalidOperationException($"Opção inválida para o produto \"{product.Name}\".");

                    var quantity = addon.Quantity < 1 ? 1 : addon.Quantity;
                    recalculatedAddons.Add(addon with { AddonName = catalogOption.Name, Price = catalogOption.AdditionalPrice, Quantity = quantity });
                    addonsTotal += catalogOption.AdditionalPrice * quantity;
                }
            }

            var unitPrice = product.Price + addonsTotal;
            subTotal += unitPrice * item.Quantity;

            pricedItems.Add(new PricedOrderItem(product.Id, product.Name, item.Quantity, unitPrice, item.Notes, recalculatedAddons));
        }

        // --- 1. Validação de Configurações ---
        var settings = await settingsRepository.GetSettingsAsync();
        
        if (!settings.IsStoreOpen)
            throw new InvalidOperationException("A loja está fechada no momento.");
            
        if (subTotal < settings.MinimumOrderAmount)
            throw new InvalidOperationException($"O pedido mínimo é de {settings.MinimumOrderAmount:C}.");

        if (type == OrderType.Delivery && !settings.AcceptingDelivery)
            throw new InvalidOperationException("A loja não está aceitando delivery no momento.");
            
        if (type == OrderType.Pickup && !settings.AcceptingPickup)
            throw new InvalidOperationException("A loja não está aceitando retirada no momento.");

        // Endereço, se informado, precisa realmente pertencer a esse cliente — nunca confiar
        // um CustomerAddressId arbitrário vindo do payload (ver RequireCustomerSession/CustomerId acima).
        if (customerAddressId.HasValue)
        {
            var customerForAddressCheck = await customerRepository.GetByIdAsync(customerId);
            var ownsAddress = customerForAddressCheck?.Addresses.Any(a => a.Id == customerAddressId.Value) ?? false;
            if (!ownsAddress)
                throw new InvalidOperationException("Endereço inválido para este cliente.");
        }

        var paymentType = await paymentTypeRepository.GetByIdAsync(paymentTypeId);
        if (paymentType == null || !paymentType.IsActive)
            throw new InvalidOperationException("Forma de pagamento inválida ou indisponível.");

        // --- 2. Calcular Taxa de Entrega ---
        decimal deliveryFee = 0;
        if (type == OrderType.Delivery)
        {
            if (settings.FreeDeliveryThreshold.HasValue && subTotal >= settings.FreeDeliveryThreshold.Value)
                deliveryFee = 0;
            else
                deliveryFee = settings.DeliveryFee;
        }

        // --- 3. Validar e Aplicar Cupom ---
        decimal discountAmount = 0;
        Guid? couponId = null;
        if (!string.IsNullOrWhiteSpace(couponCode))
        {
            var coupon = await couponRepository.GetByCodeAsync(couponCode);
            if (coupon != null && coupon.IsValid())
            {
                if (coupon.DiscountPercentage.HasValue && coupon.DiscountPercentage.Value > 0)
                {
                    discountAmount = subTotal * (coupon.DiscountPercentage.Value / 100m);
                }
                else if (coupon.DiscountFixedAmount.HasValue && coupon.DiscountFixedAmount.Value > 0)
                {
                    discountAmount = coupon.DiscountFixedAmount.Value;
                }
                couponId = coupon.Id;
                coupon.RegisterUsage();
                await couponRepository.UpdateAsync(coupon);
            }
        }

        var order = new Order(customerId, customerAddressId, type, paymentTypeId, subTotal, deliveryFee, discountAmount, couponId, couponCode, changeFor);

        foreach (var item in pricedItems)
        {
            var orderItem = new OrderItem(order.Id, item.ProductId, item.ProductName, item.Quantity, item.UnitPrice, item.Notes);

            // Salva as opções selecionadas pelo cliente (tamanho, borda, adicionais etc.) —
            // nome e preço já recalculados a partir do catálogo, nunca os do payload original.
            foreach (var addon in item.Addons)
            {
                orderItem.Addons.Add(new OrderItemAddon(
                    orderItem.Id,
                    addon.ProductOptionItemId,
                    addon.AddonName,
                    addon.Price,
                    addon.Quantity
                ));
            }

            order.Items.Add(orderItem);
        }

        await repository.AddAsync(order);
        await repository.SaveChangesAsync();

        if (paymentType.IsOnlinePayment)
            await GeneratePixChargeAsync(order);

        await NotifyCustomerStatusAsync(order);

        return order;
    }

    // Gera a cobrança Pix pro pedido recém-criado, se a forma de pagamento escolhida for online.
    // Uma falha aqui (gateway não configurado, API do Mercado Pago fora do ar, etc.) não pode
    // derrubar a criação do pedido em si — o pedido continua válido, só fica sem QR code, e o
    // lojista precisa dar atenção manual a ele (mesma postura já usada para falha de notificação).
    private async Task GeneratePixChargeAsync(Order order)
    {
        try
        {
            var customer = await customerRepository.GetByIdAsync(order.CustomerId);
            if (customer == null || string.IsNullOrWhiteSpace(customer.PhoneNumber))
                return;

            var charge = await paymentGatewayService.CreatePixChargeAsync(order.Id, order.TotalAmount, customer.PhoneNumber);
            order.SetPixCode(charge.CopyPasteCode, charge.GatewayOrderId);
            await repository.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Falha ao gerar cobrança Pix para o pedido #{OrderId}", order.Id);
        }
    }

    public async Task<IEnumerable<Order>> GetOrdersAsync()
    {
        return await repository.GetOrdersAsync();
    }

    public async Task<Order?> GetOrderByIdAsync(int orderId)
    {
        return await repository.GetByIdAsync(orderId);
    }

    public async Task<IEnumerable<Order>> GetOrdersByCustomerAsync(Guid customerId)
    {
        return await repository.GetOrdersByCustomerIdAsync(customerId);
    }

    public async Task UpdateOrderStatusAsync(int orderId, OrderStatus status)
    {
        var order = await repository.GetByIdAsync(orderId);
        if (order != null)
        {
            order.UpdateStatus(status);
            await repository.SaveChangesAsync();

            await NotifyCustomerStatusAsync(order);
        }
    }

    private async Task NotifyCustomerStatusAsync(Order order)
    {
        try
        {
            var settings = await settingsRepository.GetSettingsAsync();
            if (!settings.WhatsappBotEnabled)
                return;

            var customer = await customerRepository.GetByIdAsync(order.CustomerId);
            if (customer == null || string.IsNullOrWhiteSpace(customer.PhoneNumber))
                return;

            var shortId = order.Id.ToString();
            string message = order.Status switch
            {
                OrderStatus.New => $"🍕 *Pedido Recebido!*\n\nOlá {customer.Name ?? "Cliente"}, recebemos o seu pedido #{shortId} no valor de R$ {order.TotalAmount:F2}!\nEle já entrou na fila da nossa cozinha.",
                OrderStatus.Preparing => $"👨‍🍳 *Pedido em Preparo!*\n\nO seu pedido #{shortId} acabou de ir para o forno na nossa cozinha! Em breve sairá para entrega.",
                OrderStatus.Delivering => $"🛵 *Saiu para Entrega!*\n\nO seu pedido #{shortId} já está a caminho com o nosso motoboy! Prepare a mesa e o portão.",
                OrderStatus.Completed => $"✅ *Pedido Entregue!*\n\nO seu pedido #{shortId} foi concluído com sucesso. Bom apetite e obrigado por escolher a Pizzaria Brazil!",
                OrderStatus.Canceled => $"❌ *Pedido Cancelado:*\n\nO seu pedido #{shortId} foi cancelado. Para dúvidas ou suporte, digite 2 no menu principal do nosso WhatsApp.",
                _ => string.Empty
            };

            if (!string.IsNullOrWhiteSpace(message))
            {
                await whatsappSender.SendTextMessageAsync(customer.PhoneNumber, message);
            }
        }
        catch (Exception)
        {
            // Falhas de notificação de WhatsApp não devem impedir o fluxo do pedido
        }
    }

    // Item de pedido já com preço e opções recalculados a partir do catálogo (nunca do payload do cliente).
    private record PricedOrderItem(Guid ProductId, string ProductName, int Quantity, decimal UnitPrice, string? Notes, List<OrderItemAddonInput> Addons);
}
