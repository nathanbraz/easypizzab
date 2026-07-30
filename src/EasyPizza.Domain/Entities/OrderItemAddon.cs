using EasyPizza.Domain.Common;

namespace EasyPizza.Domain.Entities;

public class OrderItemAddon : Entity
{
    public Guid OrderItemId { get; private set; }


    /// <summary>Novo sistema: referência ao ProductOptionItem selecionado pelo cliente. Pode ser nulo.</summary>
    public Guid? ProductOptionItemId { get; private set; }

    public string AddonName { get; private set; }
    public decimal Price { get; private set; }
    public int Quantity { get; private set; }

    public OrderItem? OrderItem { get; private set; }

    protected OrderItemAddon() { AddonName = string.Empty; }


    /// <summary>Novo construtor — para opções do sistema ProductOptionItem (tamanho, borda, adicionais etc.).</summary>
    public OrderItemAddon(Guid orderItemId, Guid? productOptionItemId, string addonName, decimal price, int quantity = 1)
    {
        OrderItemId = orderItemId;
        ProductOptionItemId = productOptionItemId;
        AddonName = addonName;
        Price = price;
        Quantity = Math.Max(1, quantity);
    }
}
