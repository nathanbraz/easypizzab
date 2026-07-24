using EasyPizza.Domain.Entities;

namespace EasyPizza.Application.Interfaces.Repositories;

public interface ICouponRepository : IRepository<Coupon>
{
    Task<Coupon?> GetByCodeAsync(string code);
}
