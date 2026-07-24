using EasyPizza.Domain.Common;

namespace EasyPizza.Domain.Entities;

public class OrderItem : Entity
{
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public string? Notes { get; private set; }

    public Order? Order { get; private set; }
    public Product? Product { get; private set; }
    public ICollection<OrderItemAddon> Addons { get; private set; } = new List<OrderItemAddon>();

    public OrderItem(Guid orderId, Guid productId, int quantity, decimal unitPrice, string? notes = null)
    {
        OrderId = orderId;
        ProductId = productId;
        Quantity = quantity;
        UnitPrice = unitPrice;
        Notes = notes;
    }
}
