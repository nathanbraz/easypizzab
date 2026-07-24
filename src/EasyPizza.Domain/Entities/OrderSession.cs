using EasyPizza.Domain.Common;

namespace EasyPizza.Domain.Entities;

public class OrderSession : Entity
{
    public Guid CustomerId { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public bool IsUsed { get; private set; }

    // EF Core navigation properties
    public Customer? Customer { get; private set; }

    // Parameterless constructor for EF Core
    protected OrderSession() { }

    public OrderSession(Guid customerId, int expirationHours = 2)
    {
        CustomerId = customerId;
        ExpiresAt = DateTime.UtcNow.AddHours(expirationHours);
        IsUsed = false;
    }

    public bool IsValid()
    {
        return !IsUsed && DateTime.UtcNow <= ExpiresAt;
    }

    public void MarkAsUsed()
    {
        IsUsed = true;
        SetUpdatedAt();
    }
}
