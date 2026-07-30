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
        var categories = await _context.ProductCategories
            .Include(c => c.Products)
                .ThenInclude(p => p.OptionGroups)
                    .ThenInclude(og => og.Options)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync();


        return categories;
    }

    public async Task<Product?> GetProductByIdAsync(Guid productId)
    {
        return await _context.Products
            .Include(p => p.OptionGroups)
                .ThenInclude(og => og.Options)
            .FirstOrDefaultAsync(p => p.Id == productId);
    }
}
