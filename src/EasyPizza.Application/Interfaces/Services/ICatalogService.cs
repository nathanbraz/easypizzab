using EasyPizza.Domain.Entities;

namespace EasyPizza.Application.Interfaces.Services;

public interface ICatalogService
{
    Task<IEnumerable<ProductCategory>> GetCatalogAsync();
}
