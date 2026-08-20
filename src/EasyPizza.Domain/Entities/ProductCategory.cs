using EasyPizza.Domain.Common;

namespace EasyPizza.Domain.Entities;

public class ProductCategory : Entity
{
    public string Name { get; private set; }
    public int DisplayOrder { get; private set; }

    public ICollection<Product> Products { get; private set; } = new List<Product>();

    // Grupos de opção compartilhados por todos os produtos da categoria (ex: Tamanho, Borda,
    // Sabores) — cada produto só define seu próprio preço pra cada item, via
    // ProductCategoryOptionPrice. Se a categoria permite Meio a Meio, isso aparece como um grupo
    // com IsFlavorGroup=true aqui dentro — não é mais um bool solto na categoria.
    public ICollection<CategoryOptionGroup> OptionGroups { get; private set; } = new List<CategoryOptionGroup>();

    public ProductCategory(string name, int displayOrder = 0)
    {
        Name = name;
        DisplayOrder = displayOrder;
    }

    public void UpdateDetails(string name, int displayOrder)
    {
        Name = name;
        DisplayOrder = displayOrder;
        SetUpdatedAt();
    }
}
