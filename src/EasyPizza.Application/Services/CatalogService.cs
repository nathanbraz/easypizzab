using EasyPizza.Application.DTOs;
using EasyPizza.Application.Interfaces.Repositories;
using EasyPizza.Application.Interfaces.Services;
using EasyPizza.Domain.Entities;

namespace EasyPizza.Application.Services;

public class CatalogService : ICatalogService
{
    private readonly ICatalogRepository _repository;

    public CatalogService(ICatalogRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<CatalogCategoryDto>> GetCatalogAsync()
    {
        return await _repository.GetCategoriesWithProductsAsync();
    }

    public async Task<IEnumerable<ProductOptionGroupDto>> GetProductOptionsAsync(Guid productId)
    {
        return await _repository.GetProductOptionsAsync(productId);
    }

    public async Task<IEnumerable<CatalogProductDto>> GetAllProductsWithOptionsAsync()
    {
        return await _repository.GetAllProductsWithOptionsAsync();
    }
}
