using EasyPizza.Domain.Common;

namespace EasyPizza.Domain.Entities;

public class CustomerAddress : Entity
{
    public Guid CustomerId { get; private set; }
    public string Street { get; private set; }
    public string Number { get; private set; }
    public string Neighborhood { get; private set; }
    public string City { get; private set; }
    public string State { get; private set; }
    public string ZipCode { get; private set; }
    public string? Complement { get; private set; }
    public double? Latitude { get; private set; }
    public double? Longitude { get; private set; }
    
    // Geralmente clientes têm um endereço principal
    public bool IsDefault { get; private set; }

    public Customer? Customer { get; private set; }

    protected CustomerAddress() { }

    public CustomerAddress(Guid customerId, string street, string number, string neighborhood, string city, string state, string zipCode, string? complement = null, bool isDefault = false, double? latitude = null, double? longitude = null)
    {
        CustomerId = customerId;
        Street = street;
        Number = number;
        Neighborhood = neighborhood;
        City = city;
        State = state;
        ZipCode = zipCode;
        Complement = complement;
        IsDefault = isDefault;
        Latitude = latitude;
        Longitude = longitude;
    }

    public void SetAsDefault()
    {
        IsDefault = true;
        SetUpdatedAt();
    }
    
    public void RemoveDefault()
    {
        IsDefault = false;
        SetUpdatedAt();
    }

    public void UpdateAddress(string street, string number, string neighborhood, string city, string state, string zipCode, string? complement = null, double? latitude = null, double? longitude = null)
    {
        Street = street;
        Number = number;
        Neighborhood = neighborhood;
        City = city;
        State = state;
        ZipCode = zipCode;
        Complement = complement;
        Latitude = latitude;
        Longitude = longitude;
        SetUpdatedAt();
    }
}
