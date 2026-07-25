using EasyPizza.Domain.Entities;

namespace EasyPizza.Application.Interfaces.Services;

public interface ICustomerService
{
    Task<Customer> GetOrCreateCustomerAsync(string phoneNumber, string? name = null);
    Task<CustomerAddress?> UpdateCustomerAddressAsync(Guid customerId, string street, string number, string neighborhood, string city, string state, string zipCode, string? complement = null, double? latitude = null, double? longitude = null);
}
