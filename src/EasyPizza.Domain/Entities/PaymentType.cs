using EasyPizza.Domain.Common;

namespace EasyPizza.Domain.Entities;

public class PaymentType : Entity
{
    public string Name { get; private set; } // e.g., "Pix", "Cartão de Crédito - Maquininha", "Dinheiro"
    public bool IsOnlinePayment { get; private set; } // if true, it's Pix online. If false, it's payment at the door.
    public bool IsActive { get; private set; }
    
    // Display order on checkout
    public int DisplayOrder { get; private set; }

    protected PaymentType() { }

    public PaymentType(string name, bool isOnlinePayment, int displayOrder, bool isActive = true)
    {
        Name = name;
        IsOnlinePayment = isOnlinePayment;
        DisplayOrder = displayOrder;
        IsActive = isActive;
    }

    public void ToggleActive(bool isActive)
    {
        IsActive = isActive;
        SetUpdatedAt();
    }
}
