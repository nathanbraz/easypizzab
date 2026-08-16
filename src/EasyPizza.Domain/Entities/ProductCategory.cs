using EasyPizza.Domain.Common;

namespace EasyPizza.Domain.Entities;

public class ProductCategory : Entity
{
    public string Name { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool AllowsHalfAndHalf { get; private set; }

    public ICollection<Product> Products { get; private set; } = new List<Product>();

    // Grupos de opção compartilhados por todos os produtos da categoria (ex: Tamanho, Borda) —
    // cada produto só define seu próprio preço pra cada item, via ProductCategoryOptionPrice.
    public ICollection<CategoryOptionGroup> OptionGroups { get; private set; } = new List<CategoryOptionGroup>();

    public ProductCategory(string name, int displayOrder = 0, bool allowsHalfAndHalf = false)
    {
        Name = name;
        DisplayOrder = displayOrder;
        AllowsHalfAndHalf = allowsHalfAndHalf;
    }

    public void UpdateDetails(string name, int displayOrder, bool allowsHalfAndHalf)
    {
        Name = name;
        DisplayOrder = displayOrder;
        AllowsHalfAndHalf = allowsHalfAndHalf;
        SetUpdatedAt();
    }
}
