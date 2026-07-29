using EasyPizza.Domain.Common;

namespace EasyPizza.Domain.Entities;

public class CategoryAddon : Entity
{
    public List<Guid> CategoryIds { get; private set; } = new();
    public string Name { get; private set; }
    public decimal AdditionalPrice { get; private set; }

    public CategoryAddon(List<Guid> categoryIds, string name, decimal additionalPrice)
    {
        CategoryIds = categoryIds ?? new List<Guid>();
        Name = name;
        AdditionalPrice = additionalPrice;
    }

    public void UpdateDetails(List<Guid> categoryIds, string name, decimal additionalPrice)
    {
        CategoryIds = categoryIds ?? new List<Guid>();
        Name = name;
        AdditionalPrice = additionalPrice;
        SetUpdatedAt();
    }
}
