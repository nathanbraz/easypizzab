using EasyPizza.Domain.Common;

namespace EasyPizza.Domain.Entities;

public class Courier : Entity
{
    public string Name { get; private set; }
    public string PhoneNumber { get; private set; }
    public string? VehiclePlate { get; private set; }
    public bool IsActive { get; private set; }

    protected Courier() { }

    public Courier(string name, string phoneNumber, string? vehiclePlate)
    {
        Name = name;
        PhoneNumber = phoneNumber;
        VehiclePlate = vehiclePlate;
        IsActive = true;
    }

    public void UpdateDetails(string name, string phoneNumber, string? vehiclePlate, bool isActive)
    {
        Name = name;
        PhoneNumber = phoneNumber;
        VehiclePlate = vehiclePlate;
        IsActive = isActive;
        SetUpdatedAt();
    }
}
