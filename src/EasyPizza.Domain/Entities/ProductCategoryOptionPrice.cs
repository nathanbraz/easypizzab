using EasyPizza.Domain.Common;

namespace EasyPizza.Domain.Entities;

// Liga um produto a um CategoryOptionItem, com o preço específico desse produto naquela opção
// (nulo se o grupo tiver preço uniforme — nesse caso o preço vem de CategoryOptionItem.UniformPrice).
// IsOffered controla se o produto oferece a opção agora — desmarcar não apaga a linha, só marca
// IsOffered=false, pra manter o preço configurado caso o lojista marque de novo depois. A linha só
// deixa de existir de fato se o item ou o produto forem excluídos (cascata).
public class ProductCategoryOptionPrice : Entity
{
    public Guid ProductId { get; private set; }
    public Guid CategoryOptionItemId { get; private set; }
    public decimal? AdditionalPrice { get; private set; }
    public bool IsOffered { get; private set; }

    public Product? Product { get; private set; }
    public CategoryOptionItem? CategoryOptionItem { get; private set; }

    protected ProductCategoryOptionPrice() { }

    public ProductCategoryOptionPrice(Guid productId, Guid categoryOptionItemId, decimal? additionalPrice)
    {
        ProductId = productId;
        CategoryOptionItemId = categoryOptionItemId;
        AdditionalPrice = additionalPrice;
        IsOffered = true;
    }

    public void UpdatePrice(decimal? additionalPrice)
    {
        AdditionalPrice = additionalPrice;
        IsOffered = true;
        SetUpdatedAt();
    }

    public void SetOffered(bool offered)
    {
        IsOffered = offered;
        SetUpdatedAt();
    }
}
