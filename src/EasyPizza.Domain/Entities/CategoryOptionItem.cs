using EasyPizza.Domain.Common;

namespace EasyPizza.Domain.Entities;

// Valor possível dentro de um CategoryOptionGroup (ex: "G", "Catupiry"). Se o grupo NÃO tem preço
// uniforme (ex: Tamanho), o preço fica por conta de cada produto via ProductCategoryOptionPrice —
// a ausência de uma linha lá significa que o produto não oferece esse item. Se o grupo TEM preço
// uniforme (ex: Borda), o preço é este UniformPrice, o mesmo pra todo produto que oferecer o item —
// a linha em ProductCategoryOptionPrice ainda controla se o produto oferece ou não, só o preço dela
// deixa de ser usado.
public class CategoryOptionItem : Entity
{
    public Guid GroupId { get; private set; }
    public string Name { get; private set; }
    public int DisplayOrder { get; private set; }
    public decimal? UniformPrice { get; private set; }

    // Só usado quando o grupo é IsFlavorGroup=true: qual Produto da categoria este item
    // representa (ex: item "Calabresa" -> Produto "Pizza Calabresa"). Nome/foto do item
    // seguem o Produto linkado nesse caso, em vez de serem digitados à mão.
    public Guid? ProductId { get; private set; }

    public CategoryOptionGroup? Group { get; private set; }
    public Product? Product { get; private set; }

    protected CategoryOptionItem() { }

    public CategoryOptionItem(Guid groupId, string name, int displayOrder = 0, decimal? uniformPrice = null, Guid? productId = null)
    {
        GroupId = groupId;
        Name = name;
        DisplayOrder = displayOrder;
        UniformPrice = uniformPrice;
        ProductId = productId;
    }

    public void UpdateDetails(string name, int displayOrder, decimal? uniformPrice, Guid? productId)
    {
        Name = name;
        DisplayOrder = displayOrder;
        UniformPrice = uniformPrice;
        ProductId = productId;
        SetUpdatedAt();
    }
}
