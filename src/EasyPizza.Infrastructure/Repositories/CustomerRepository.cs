using EasyPizza.Application.Interfaces.Repositories;
using EasyPizza.Domain.Entities;
using EasyPizza.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EasyPizza.Infrastructure.Repositories;

public class CustomerRepository : Repository<Customer>, ICustomerRepository
{
    public CustomerRepository(EasyPizzaDbContext context) : base(context)
    {
    }

    public async Task<Customer?> GetByPhoneNumberAsync(string phoneNumber)
    {
        return await _dbSet.Include(c => c.Addresses).FirstOrDefaultAsync(c => c.PhoneNumber == phoneNumber);
    }

    public override async Task<Customer?> GetByIdAsync(Guid id)
    {
        return await _dbSet.Include(c => c.Addresses).FirstOrDefaultAsync(c => c.Id == id);
    }
}
