using EasyPizza.Domain.Common;

namespace EasyPizza.Domain.Entities;

public class ProductAddon : Entity
{
    public Guid ProductId { get; private set; }
    public string Name { get; private set; }
    public decimal AdditionalPrice { get; private set; }

    public Product? Product { get; private set; }

    public ProductAddon(Guid productId, string name, decimal additionalPrice)
    {
        ProductId = productId;
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
