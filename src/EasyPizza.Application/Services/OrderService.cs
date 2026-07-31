using EasyPizza.Application.Interfaces.Repositories;
using EasyPizza.Application.Interfaces.Services;
using EasyPizza.Domain.Entities;

namespace EasyPizza.Application.Services;

public class OrderService(
    IOrderRepository repository,
    IStoreSettingsRepository settingsRepository,
    IPaymentTypeRepository paymentTypeRepository,
    ICouponRepository couponRepository,
    ICustomerRepository customerRepository,
    IWhatsappSender whatsappSender,
    ICatalogRepository catalogRepository) : IOrderService
{
    public async Task<Order> CreateOrderAsync(Guid customerId, Guid? customerAddressId, OrderType type, Guid paymentTypeId, List<OrderItemInput> items, string? couponCode = null, decimal? changeFor = null)
    {
        var subTotal = items.Sum(i => i.Quantity * i.UnitPrice);
        
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

        foreach (var item in items)
        {
            var product = await catalogRepository.GetProductByIdAsync(item.ProductId);
            string productName = product?.Name ?? "Produto Desconhecido";

            var orderItem = new OrderItem(order.Id, item.ProductId, productName, item.Quantity, item.UnitPrice, item.Notes);

            // Salva as opções selecionadas pelo cliente (tamanho, borda, adicionais etc.)
            if (item.Addons != null)
            {
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
            }

            order.Items.Add(orderItem);
        }

        await repository.AddAsync(order);
        await repository.SaveChangesAsync();

        await NotifyCustomerStatusAsync(order);

        return order;
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
}
