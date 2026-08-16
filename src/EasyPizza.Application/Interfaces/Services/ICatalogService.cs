using EasyPizza.Application.DTOs;
using EasyPizza.Domain.Entities;

namespace EasyPizza.Application.Interfaces.Services;

public interface ICatalogService
{
    Task<IEnumerable<CatalogCategoryDto>> GetCatalogAsync();
    Task<IEnumerable<ProductOptionGroupDto>> GetProductOptionsAsync(Guid productId);
    Task<IEnumerable<CatalogProductDto>> GetAllProductsWithOptionsAsync();
}
