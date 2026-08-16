using EasyPizza.Application.DTOs;
using EasyPizza.Domain.Entities;

namespace EasyPizza.Application.Interfaces.Repositories;

public interface ICatalogRepository
{
    Task<IEnumerable<CatalogCategoryDto>> GetCategoriesWithProductsAsync();
    Task<Product?> GetProductByIdAsync(Guid productId);

    // Junta os grupos próprios do produto (ex: Adicionais extras) com os grupos compartilhados
    // da categoria (Tamanho, Borda) — só os itens que esse produto especificamente oferece.
    Task<IEnumerable<ProductOptionGroupDto>> GetProductOptionsAsync(Guid productId);

    // Todos os produtos (inclusive indisponíveis) já com as opções mescladas — usado pela gestão
    // de catálogo no admin, pra mostrar o preço real (ex: "A partir de R$35") em vez do preço
    // bruto do produto.
    Task<IEnumerable<CatalogProductDto>> GetAllProductsWithOptionsAsync();
}
