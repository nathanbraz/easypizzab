using EasyPizza.Domain.Common;

namespace EasyPizza.Domain.Entities;

public class CategoryAddon : Entity
{
    public Guid CategoryId { get; private set; }
    public string Name { get; private set; }
    public decimal AdditionalPrice { get; private set; }

    public ProductCategory? Category { get; private set; }

    public CategoryAddon(Guid categoryId, string name, decimal additionalPrice)
    {
        CategoryId = categoryId;
        Name = name;
        AdditionalPrice = additionalPrice;
    }

    public void UpdateDetails(string name, decimal additionalPrice)
    {
        Name = name;
        AdditionalPrice = additionalPrice;
        SetUpdatedAt();
    }
}
