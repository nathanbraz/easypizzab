using EasyPizza.Domain.Entities;

namespace EasyPizza.Application.Interfaces.Services;

public interface IOrderService
{
    Task<Order> CreateOrderAsync(Guid customerId, Guid? customerAddressId, OrderType type, Guid paymentTypeId, List<(Guid productId, int quantity, decimal unitPrice)> items);
    Task<IEnumerable<Order>> GetOrdersAsync();
    Task UpdateOrderStatusAsync(Guid orderId, OrderStatus status);
}
