using EasyPizza.Domain.Entities;

namespace EasyPizza.Application.Interfaces.Services;

public interface IOrderService
{
    Task<Order> CreateOrderAsync(Guid customerId, Guid? customerAddressId, OrderType type, Guid paymentTypeId, List<(Guid productId, int quantity, decimal unitPrice)> items, string? couponCode = null);
    Task<IEnumerable<Order>> GetOrdersAsync();
    Task<Order?> GetOrderByIdAsync(int orderId);
    Task<IEnumerable<Order>> GetOrdersByCustomerAsync(Guid customerId);
    Task UpdateOrderStatusAsync(int orderId, OrderStatus status);
}
