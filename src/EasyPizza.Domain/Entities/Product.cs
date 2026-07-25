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

    public ProductCategory? Category { get; private set; }

    public Product(Guid categoryId, string name, string description, decimal price)
    {
        CategoryId = categoryId;
        Name = name;
        Description = description;
        Price = price;
        IsAvailable = true;
    }

    public void UpdateDetails(string name, string description, decimal price, List<string> imageUrls, bool isAvailable)
    {
        Name = name;
        Description = description;
        Price = price;
        ImageUrls = imageUrls ?? new List<string>();
        IsAvailable = isAvailable;
        SetUpdatedAt();
    }
}
