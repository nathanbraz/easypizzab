using EasyPizza.Domain.Entities;

namespace EasyPizza.Application.Interfaces.Repositories;

public interface IOrderRepository : IRepository<Order>
{
    Task<Order?> GetByIdAsync(int id);
    Task<IEnumerable<Order>> GetOrdersAsync();
    Task<IEnumerable<Order>> GetOrdersByCustomerIdAsync(Guid customerId);
    Task<Order?> GetLastCustomerOrderAsync(Guid customerId);
}
