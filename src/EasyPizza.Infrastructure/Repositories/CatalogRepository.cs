using EasyPizza.Application.Interfaces.Repositories;
using EasyPizza.Domain.Entities;
using EasyPizza.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EasyPizza.Infrastructure.Repositories;

public class CatalogRepository : ICatalogRepository
{
    private readonly EasyPizzaDbContext _context;

    public CatalogRepository(EasyPizzaDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ProductCategory>> GetCategoriesWithProductsAsync()
    {
        return await _context.ProductCategories
            .Include(c => c.Products)
            .ThenInclude(p => p.Addons)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync();
    }

    public async Task<Product?> GetProductByIdAsync(Guid productId)
    {
        return await _context.Products
            .Include(p => p.Addons)
            .FirstOrDefaultAsync(p => p.Id == productId);
    }
}
