using EasyPizza.Domain.Common;

namespace EasyPizza.Domain.Entities;

public class ProductOptionGroup : Entity
{
    public Guid ProductId { get; private set; }
    public string Name { get; private set; }
    public string GroupType { get; private set; }
    public bool IsRequired { get; private set; }
    public int MinChoices { get; private set; }
    public int MaxChoices { get; private set; }
    public int DisplayOrder { get; private set; }

    public Product? Product { get; private set; }
    public ICollection<ProductOptionItem> Options { get; private set; } = new List<ProductOptionItem>();

    public ProductOptionGroup(Guid productId, string name, string groupType, bool isRequired, int minChoices, int maxChoices, int displayOrder = 0)
    {
        ProductId = productId;
        Name = name;
        GroupType = groupType;
        IsRequired = isRequired;
        MinChoices = minChoices;
        MaxChoices = maxChoices;
        DisplayOrder = displayOrder;
    }

    public void UpdateDetails(string name, string groupType, bool isRequired, int minChoices, int maxChoices, int displayOrder)
    {
        Name = name;
        GroupType = groupType;
        IsRequired = isRequired;
        MinChoices = minChoices;
        MaxChoices = maxChoices;
        DisplayOrder = displayOrder;
        SetUpdatedAt();
    }
}
