using EasyPizza.Domain.Common;

namespace EasyPizza.Domain.Entities;

public class Customer : Entity
{
    public string PhoneNumber { get; private set; }
    public string? Name { get; private set; }
    
    // 1-N Navigation property
    public ICollection<CustomerAddress> Addresses { get; private set; } = new List<CustomerAddress>();
    public ICollection<OrderSession> Sessions { get; private set; } = new List<OrderSession>();

    // Parameterless constructor for EF Core
    protected Customer() { }

    public Customer(string phoneNumber, string? name = null)
    {
        PhoneNumber = phoneNumber;
        Name = name;
    }

    public void UpdateName(string name)
    {
        Name = name;
        SetUpdatedAt();
    }
}
