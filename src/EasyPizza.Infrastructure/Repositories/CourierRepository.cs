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

    public async Task<IEnumerable<Courier>> GetActiveCouriersAsync()
    {
        return await _dbSet.Where(c => c.IsActive).ToListAsync();
    }
}
