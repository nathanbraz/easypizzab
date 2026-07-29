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
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync();

        var addons = await _context.CategoryAddons.ToListAsync();

        foreach (var category in categories)
        {
            category.Addons = addons.Where(a => a.CategoryIds != null && a.CategoryIds.Contains(category.Id)).ToList();
        }

        return categories;
    }

    public async Task<Product?> GetProductByIdAsync(Guid productId)
    {
        return await _context.Products
            .FirstOrDefaultAsync(p => p.Id == productId);
    }
}
