using EasyPizza.Domain.Common;

namespace EasyPizza.Domain.Entities;

public class PaymentType : Entity
{
    public string Name { get; private set; } // ex., "Pix", "Cartão de Crédito - Maquininha", "Dinheiro"
    public bool IsOnlinePayment { get; private set; } // se verdadeiro, é Pix online. Se falso, é pagamento na porta.
    public bool IsActive { get; private set; }
    
    // Ordem de exibição no checkout
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
