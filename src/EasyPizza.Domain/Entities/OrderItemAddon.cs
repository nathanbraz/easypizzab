using EasyPizza.Domain.Common;

namespace EasyPizza.Domain.Entities;

public class OrderItemAddon : Entity
{
    public Guid OrderItemId { get; private set; }
    public Guid ProductAddonId { get; private set; }
    public string AddonName { get; private set; }
    public decimal Price { get; private set; }

    public OrderItem? OrderItem { get; private set; }
    public ProductAddon? ProductAddon { get; private set; }

    public OrderItemAddon(Guid orderItemId, Guid productAddonId, string addonName, decimal price)
    {
        OrderItemId = orderItemId;
        ProductAddonId = productAddonId;
        AddonName = addonName;
        Price = price;
    }
}
