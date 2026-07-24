using EasyPizza.Application.Interfaces.Repositories;
using EasyPizza.Application.Interfaces.Services;
using EasyPizza.Domain.Entities;

namespace EasyPizza.Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _repository;

    public OrderService(IOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task<Order> CreateOrderAsync(Guid customerId, Guid customerAddressId, Guid paymentTypeId, List<(Guid productId, int quantity, decimal unitPrice)> items, decimal deliveryFee)
    {
        var subTotal = items.Sum(i => i.quantity * i.unitPrice);
        var order = new Order(customerId, customerAddressId, paymentTypeId, subTotal, deliveryFee);

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
