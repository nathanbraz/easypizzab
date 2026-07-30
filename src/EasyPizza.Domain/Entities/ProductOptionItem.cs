using EasyPizza.Domain.Common;

namespace EasyPizza.Domain.Entities;

public class ProductOptionItem : Entity
{
    public Guid GroupId { get; private set; }
    public string Name { get; private set; }
    public decimal AdditionalPrice { get; private set; }
    public int DisplayOrder { get; private set; }

    public ProductOptionGroup? Group { get; private set; }

    public ProductOptionItem(Guid groupId, string name, decimal additionalPrice = 0, int displayOrder = 0)
    {
        GroupId = groupId;
        Name = name;
        AdditionalPrice = additionalPrice;
        DisplayOrder = displayOrder;
    }

    public void UpdateDetails(string name, decimal additionalPrice, int displayOrder)
    {
        Name = name;
        AdditionalPrice = additionalPrice;
        DisplayOrder = displayOrder;
        SetUpdatedAt();
    }
}
