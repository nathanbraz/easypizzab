using EasyPizza.Domain.Common;

namespace EasyPizza.Domain.Entities;

public class OrderItemAddon : Entity
{
    public Guid OrderItemId { get; private set; }
    public Guid CategoryAddonId { get; private set; }
    public string AddonName { get; private set; }
    public decimal Price { get; private set; }

    public OrderItem? OrderItem { get; private set; }
    public CategoryAddon? CategoryAddon { get; private set; }

    public OrderItemAddon(Guid orderItemId, Guid categoryAddonId, string addonName, decimal price)
    {
        OrderItemId = orderItemId;
        CategoryAddonId = categoryAddonId;
        AddonName = addonName;
        Price = price;
    }
}
