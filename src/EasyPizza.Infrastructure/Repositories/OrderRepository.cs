using EasyPizza.Application.Interfaces.Repositories;
using EasyPizza.Domain.Entities;
using EasyPizza.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EasyPizza.Infrastructure.Repositories;

public class OrderRepository : Repository<Order>, IOrderRepository
{
    public OrderRepository(EasyPizzaDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Order>> GetOrdersAsync()
    {
        return await _dbSet
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .Include(o => o.Items)
            .ThenInclude(i => i.Addons)
            .Include(o => o.Customer)
            .Include(o => o.Address)
            .Include(o => o.PaymentType)
            .Include(o => o.Coupon)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetOrdersByCustomerIdAsync(Guid customerId)
    {
        return await _dbSet
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .Include(o => o.Items)
            .ThenInclude(i => i.Addons)
            .Include(o => o.Customer)
            .Include(o => o.Address)
            .Include(o => o.PaymentType)
            .Include(o => o.Coupon)
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<Order?> GetByIdAsync(int orderId)
    {
        return await _dbSet
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .Include(o => o.Items)
            .ThenInclude(i => i.Addons)
            .Include(o => o.Customer)
            .Include(o => o.Address)
            .Include(o => o.PaymentType)
            .Include(o => o.Coupon)
            .FirstOrDefaultAsync(o => o.Id == orderId);
    }

    public override async Task<Order?> GetByIdAsync(Guid orderId)
    {
        return await Task.FromResult<Order?>(null);
    }

    public async Task<Order?> GetLastCustomerOrderAsync(Guid customerId)
    {
        return await _dbSet
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .Include(o => o.Items)
            .ThenInclude(i => i.Addons)
            .Include(o => o.Customer)
            .Include(o => o.Address)
            .Include(o => o.PaymentType)
            .Include(o => o.Coupon)
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync();
    }
}
