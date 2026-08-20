using EasyPizza.Application.DTOs;
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

    public async Task<IEnumerable<ProductOptionGroupDto>> GetProductOptionsAsync(Guid productId)
    {
        var result = new List<ProductOptionGroupDto>();

        // Grupos próprios do produto (ex: Adicionais extras) — sem mudança em relação ao que já existia.
        var ownGroups = await _context.ProductOptionGroups
            .Where(g => g.ProductId == productId)
            .Include(g => g.Options)
            .ToListAsync();

        result.AddRange(ownGroups.Select(g => new ProductOptionGroupDto
        {
            Id = g.Id,
            Name = g.Name,
            GroupType = g.GroupType,
            IsRequired = g.IsRequired,
            MinChoices = g.MinChoices,
            MaxChoices = g.MaxChoices,
            DisplayOrder = g.DisplayOrder,
            IsShared = false,
            Options = g.Options.OrderBy(o => o.DisplayOrder).Select(o => new ProductOptionItemDto
            {
                Id = o.Id,
                Name = o.Name,
                AdditionalPrice = o.AdditionalPrice,
                DisplayOrder = o.DisplayOrder
            }).ToList()
        }));

        // Grupos compartilhados da categoria (Tamanho, Borda) — só os itens que ESSE produto
        // realmente oferece agora (linha em ProductCategoryOptionPrices com IsOffered=true). Uma
        // linha com IsOffered=false existe (guarda o preço pra quando reativar), mas não conta aqui.
        var sharedRows = await _context.ProductCategoryOptionPrices
            .Where(price => price.ProductId == productId && price.IsOffered)
            .Select(price => new
            {
                price.AdditionalPrice,
                ItemId = price.CategoryOptionItem!.Id,
                ItemName = price.CategoryOptionItem!.Name,
                ItemDisplayOrder = price.CategoryOptionItem!.DisplayOrder,
                ItemUniformPrice = price.CategoryOptionItem!.UniformPrice,
                ItemProductId = price.CategoryOptionItem!.ProductId,
                GroupId = price.CategoryOptionItem!.Group!.Id,
                GroupName = price.CategoryOptionItem!.Group!.Name,
                GroupMin = price.CategoryOptionItem!.Group!.MinChoices,
                GroupMax = price.CategoryOptionItem!.Group!.MaxChoices,
                GroupDisplayOrder = price.CategoryOptionItem!.Group!.DisplayOrder,
                GroupHasUniformPricing = price.CategoryOptionItem!.Group!.HasUniformPricing,
                GroupIsFlavorGroup = price.CategoryOptionItem!.Group!.IsFlavorGroup,
                GroupFlavorPriceStrategy = price.CategoryOptionItem!.Group!.FlavorPriceStrategy
            })
            // No grupo de Sabores, um produto nunca oferece a si mesmo como "sabor extra".
            .Where(x => x.ItemProductId == null || x.ItemProductId != productId)
            .ToListAsync();

        result.AddRange(sharedRows
            .GroupBy(x => new { x.GroupId, x.GroupName, x.GroupMin, x.GroupMax, x.GroupDisplayOrder, x.GroupIsFlavorGroup, x.GroupFlavorPriceStrategy })
            .Select(g => new ProductOptionGroupDto
            {
                Id = g.Key.GroupId,
                Name = g.Key.GroupName,
                GroupType = "single",
                IsRequired = g.Key.GroupMin > 0,
                MinChoices = g.Key.GroupMin,
                MaxChoices = g.Key.GroupMax,
                DisplayOrder = g.Key.GroupDisplayOrder,
                IsShared = true,
                IsFlavorGroup = g.Key.GroupIsFlavorGroup,
                FlavorPriceStrategy = g.Key.GroupFlavorPriceStrategy,
                Options = g.OrderBy(x => x.ItemDisplayOrder).Select(x => new ProductOptionItemDto
                {
                    Id = x.ItemId,
                    Name = x.ItemName,
                    // Grupo de preço uniforme (ex: Borda): sempre o preço atual do item, nunca o
                    // valor gravado na linha do produto — assim mudar o preço uma vez na categoria
                    // já reflete em todo produto que oferece o item, sem precisar tocar em cada um.
                    AdditionalPrice = (x.GroupHasUniformPricing ? x.ItemUniformPrice : x.AdditionalPrice) ?? 0,
                    DisplayOrder = x.ItemDisplayOrder,
                    LinkedProductId = x.ItemProductId
                }).ToList()
            }));

        return result.OrderBy(g => g.DisplayOrder).ToList();
    }

    public async Task<IEnumerable<CatalogCategoryDto>> GetCategoriesWithProductsAsync()
    {
        // Cardápio público: só produtos marcados como disponíveis pelo lojista.
        // (A gestão do catálogo no admin usa outro endpoint, sem esse filtro.)
        var categories = await _context.ProductCategories
            .Include(c => c.Products.Where(p => p.IsAvailable))
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync();

        var result = new List<CatalogCategoryDto>();
        foreach (var category in categories)
        {
            var categoryDto = new CatalogCategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                DisplayOrder = category.DisplayOrder
            };

            foreach (var product in category.Products.OrderBy(p => p.Name))
            {
                categoryDto.Products.Add(await BuildProductDtoAsync(product));
            }

            result.Add(categoryDto);
        }

        return result;
    }

    public async Task<IEnumerable<CatalogProductDto>> GetAllProductsWithOptionsAsync()
    {
        // Gestão do catálogo no admin: TODOS os produtos (inclusive indisponíveis), com as mesmas
        // opções mescladas (próprias + compartilhadas da categoria) que o cardápio do cliente usa —
        // pra "Preço Base" na listagem do admin mostrar exatamente o mesmo valor que o cliente vê,
        // em vez do preço bruto do produto (que pode ser R$0 quando o preço vem do Tamanho).
        var products = await _context.Products.OrderBy(p => p.Name).ToListAsync();

        var result = new List<CatalogProductDto>();
        foreach (var product in products)
        {
            result.Add(await BuildProductDtoAsync(product));
        }

        return result;
    }

    private async Task<CatalogProductDto> BuildProductDtoAsync(Product product)
    {
        return new CatalogProductDto
        {
            Id = product.Id,
            CategoryId = product.CategoryId,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            ImageUrls = product.ImageUrls,
            IsAvailable = product.IsAvailable,
            ShowInCrossSell = product.ShowInCrossSell,
            CrossSellDiscountPrice = product.CrossSellDiscountPrice,
            // Mesma combinação (grupos próprios + compartilhados da categoria) do endpoint de detalhe.
            OptionGroups = (await GetProductOptionsAsync(product.Id)).ToList()
        };
    }

    public async Task<Product?> GetProductByIdAsync(Guid productId)
    {
        return await _context.Products
            .Include(p => p.OptionGroups)
                .ThenInclude(og => og.Options)
            .FirstOrDefaultAsync(p => p.Id == productId);
    }
}
