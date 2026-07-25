using EasyPizza.Domain.Common;

namespace EasyPizza.Domain.Entities;

public class ProductCategory : Entity
{
    public string Name { get; private set; }
    public int DisplayOrder { get; private set; }

    public ICollection<Product> Products { get; private set; } = new List<Product>();
    public ICollection<CategoryAddon> Addons { get; private set; } = new List<CategoryAddon>();

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
