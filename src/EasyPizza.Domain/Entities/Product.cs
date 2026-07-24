using EasyPizza.Domain.Common;

namespace EasyPizza.Domain.Entities;

public class Product : Entity
{
    public Guid CategoryId { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public decimal Price { get; private set; }
    public string? ImageUrl { get; private set; }
    public bool IsAvailable { get; private set; }

    public ProductCategory? Category { get; private set; }
    public ICollection<ProductAddon> Addons { get; private set; } = new List<ProductAddon>();

    public Product(Guid categoryId, string name, string description, decimal price)
    {
        CategoryId = categoryId;
        Name = name;
        Description = description;
        Price = price;
        IsAvailable = true;
    }

    public void UpdateDetails(string name, string description, decimal price)
    {
        Name = name;
        Description = description;
        Price = price;
        SetUpdatedAt();
    }
}
