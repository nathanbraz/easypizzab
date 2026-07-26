using EasyPizza.Application.Interfaces.Repositories;
using EasyPizza.Application.Interfaces.Services;
using EasyPizza.Domain.Entities;

namespace EasyPizza.Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _repository;
    private readonly IStoreSettingsRepository _settingsRepository;
    private readonly IPaymentTypeRepository _paymentTypeRepository;
    private readonly ICouponRepository _couponRepository;

    public OrderService(IOrderRepository repository, IStoreSettingsRepository settingsRepository, IPaymentTypeRepository paymentTypeRepository, ICouponRepository couponRepository)
    {
        _repository = repository;
        _settingsRepository = settingsRepository;
        _paymentTypeRepository = paymentTypeRepository;
        _couponRepository = couponRepository;
    }

    public async Task<Order> CreateOrderAsync(Guid customerId, Guid? customerAddressId, OrderType type, Guid paymentTypeId, List<(Guid productId, int quantity, decimal unitPrice)> items, string? couponCode = null)
    {
        var subTotal = items.Sum(i => i.quantity * i.unitPrice);
        
        // --- 1. Settings Validation ---
        var settings = await _settingsRepository.GetSettingsAsync();
        
        if (!settings.IsStoreOpen)
            throw new InvalidOperationException("A loja está fechada no momento.");
            
        if (subTotal < settings.MinimumOrderAmount)
            throw new InvalidOperationException($"O pedido mínimo é de {settings.MinimumOrderAmount:C}.");

        if (type == OrderType.Delivery && !settings.AcceptingDelivery)
            throw new InvalidOperationException("A loja não está aceitando delivery no momento.");
            
        if (type == OrderType.Pickup && !settings.AcceptingPickup)
            throw new InvalidOperationException("A loja não está aceitando retirada no momento.");

        var paymentType = await _paymentTypeRepository.GetByIdAsync(paymentTypeId);
        if (paymentType == null || !paymentType.IsActive)
            throw new InvalidOperationException("Forma de pagamento inválida ou indisponível.");

        // --- 2. Calculate Delivery Fee ---
        decimal deliveryFee = 0;
        if (type == OrderType.Delivery)
        {
            if (settings.FreeDeliveryThreshold.HasValue && subTotal >= settings.FreeDeliveryThreshold.Value)
                deliveryFee = 0;
            else
                deliveryFee = settings.DeliveryFee;
        }

        // --- 3. Validate and Apply Coupon ---
        decimal discountAmount = 0;
        Guid? couponId = null;
        if (!string.IsNullOrWhiteSpace(couponCode))
        {
            var coupon = await _couponRepository.GetByCodeAsync(couponCode);
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
                await _couponRepository.UpdateAsync(coupon);
            }
        }

        var order = new Order(customerId, customerAddressId, type, paymentTypeId, subTotal, deliveryFee, discountAmount, couponId, couponCode);

        foreach (var item in items)
        {
            order.Items.Add(new OrderItem(order.Id, item.productId, item.quantity, item.unitPrice));
        }

        await _repository.AddAsync(order);
        await _repository.SaveChangesAsync();

        return order;
    }

    public async Task<IEnumerable<Order>> GetOrdersAsync()
    {
        return await _repository.GetOrdersAsync();
    }

    public async Task<Order?> GetOrderByIdAsync(Guid orderId)
    {
        return await _repository.GetByIdAsync(orderId);
    }

    public async Task<IEnumerable<Order>> GetOrdersByCustomerAsync(Guid customerId)
    {
        return await _repository.GetOrdersByCustomerIdAsync(customerId);
    }

    public async Task UpdateOrderStatusAsync(Guid orderId, OrderStatus status)
    {
        var order = await _repository.GetByIdAsync(orderId);
        if (order != null)
        {
            order.UpdateStatus(status);
            await _repository.SaveChangesAsync();
        }
    }
}
