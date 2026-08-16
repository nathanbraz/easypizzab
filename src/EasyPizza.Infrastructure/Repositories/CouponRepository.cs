using EasyPizza.Application.Interfaces.Repositories;
using EasyPizza.Domain.Entities;
using EasyPizza.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EasyPizza.Infrastructure.Repositories;

public class CouponRepository : Repository<Coupon>, ICouponRepository
{
    public CouponRepository(EasyPizzaDbContext context) : base(context)
    {
    }

    public override async Task<IEnumerable<Coupon>> GetAllAsync()
    {
        return await _dbSet.OrderByDescending(c => c.CreatedAt).ToListAsync();
    }

    public async Task<Coupon?> GetByCodeAsync(string code)
    {
        return await _dbSet.FirstOrDefaultAsync(c => c.Code == code.ToUpperInvariant());
    }
}
