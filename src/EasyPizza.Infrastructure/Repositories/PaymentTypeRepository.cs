using EasyPizza.Application.Interfaces.Repositories;
using EasyPizza.Domain.Entities;
using EasyPizza.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EasyPizza.Infrastructure.Repositories;

public class PaymentTypeRepository : IPaymentTypeRepository
{
    private readonly EasyPizzaDbContext _context;

    public PaymentTypeRepository(EasyPizzaDbContext context)
    {
        _context = context;
    }

    public async Task<PaymentType?> GetByIdAsync(Guid id)
    {
        return await _context.PaymentTypes.FindAsync(id);
    }

    public async Task<IEnumerable<PaymentType>> GetAllActiveAsync()
    {
        return await _context.PaymentTypes
            .Where(pt => pt.IsActive)
            .OrderBy(pt => pt.DisplayOrder)
            .ToListAsync();
    }
}
