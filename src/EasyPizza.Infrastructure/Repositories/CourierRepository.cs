using EasyPizza.Application.Interfaces.Repositories;
using EasyPizza.Domain.Entities;
using EasyPizza.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EasyPizza.Infrastructure.Repositories;

public class CourierRepository : Repository<Courier>, ICourierRepository
{
    public CourierRepository(EasyPizzaDbContext dbContext) : base(dbContext)
    {
    }

    public override async Task<IEnumerable<Courier>> GetAllAsync()
    {
        return await _dbSet.OrderBy(c => c.Name).ToListAsync();
    }

    public async Task<IEnumerable<Courier>> GetActiveCouriersAsync()
    {
        return await _dbSet.Where(c => c.IsActive).OrderBy(c => c.Name).ToListAsync();
    }
}
