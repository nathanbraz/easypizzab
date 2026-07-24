using EasyPizza.Domain.Entities;

namespace EasyPizza.Application.Interfaces.Repositories;

public interface IOrderRepository : IRepository<Order>
{
    Task<IEnumerable<Order>> GetOrdersAsync();
}
