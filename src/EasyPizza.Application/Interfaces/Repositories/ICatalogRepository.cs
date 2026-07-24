using EasyPizza.Domain.Entities;

namespace EasyPizza.Application.Interfaces.Repositories;

public interface ICatalogRepository
{
    Task<IEnumerable<ProductCategory>> GetCategoriesWithProductsAsync();
    Task<Product?> GetProductByIdAsync(Guid productId);
}
