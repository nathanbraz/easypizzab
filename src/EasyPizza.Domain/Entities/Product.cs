using EasyPizza.Domain.Common;

namespace EasyPizza.Domain.Entities;

public class Product : Entity
{
    public Guid CategoryId { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public decimal Price { get; private set; }
    public List<string> ImageUrls { get; private set; } = new();
    public bool IsAvailable { get; private set; }
    public bool ShowInCrossSell { get; private set; }

    // Preço cobrado quando este produto é adicionado via sugestão de cross-sell (carrossel
    // "Aproveite e leve também" no checkout) em vez do preço normal do catálogo. Nulo = sem
    // desconto, usa o Price normal mesmo quando sugerido. Só o backend decide o preço final
    // (ver OrderService) — o cliente nunca manda o valor do desconto, só sinaliza que esse item
    // veio do fluxo de sugestão.
    public decimal? CrossSellDiscountPrice { get; private set; }

    public ProductCategory? Category { get; private set; }

    public ICollection<ProductOptionGroup> OptionGroups { get; private set; } = new List<ProductOptionGroup>();

    // Preço deste produto pra cada item de opção compartilhada da categoria (Tamanho, Borda) que
    // ele realmente oferece. Sem linha aqui pra um item = esse produto não oferece aquela opção.
    public ICollection<ProductCategoryOptionPrice> CategoryOptionPrices { get; private set; } = new List<ProductCategoryOptionPrice>();

    public Product(Guid categoryId, string name, string description, decimal price)
    {
        CategoryId = categoryId;
        Name = name;
        Description = description;
        Price = price;
        IsAvailable = true;
        ShowInCrossSell = false;
    }

    public void UpdateDetails(string name, string description, decimal price, List<string> imageUrls, bool isAvailable, bool showInCrossSell = false, decimal? crossSellDiscountPrice = null)
    {
        Name = name;
        Description = description;
        Price = price;
        ImageUrls = imageUrls ?? new List<string>();
        IsAvailable = isAvailable;
        ShowInCrossSell = showInCrossSell;
        CrossSellDiscountPrice = crossSellDiscountPrice;
        SetUpdatedAt();
    }
}
