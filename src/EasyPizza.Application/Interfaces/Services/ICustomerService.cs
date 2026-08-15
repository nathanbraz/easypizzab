using EasyPizza.Domain.Entities;

namespace EasyPizza.Application.Interfaces.Services;

public interface ICustomerService
{
    Task<Customer> GetOrCreateCustomerAsync(string phoneNumber, string? name = null);

    Task<IEnumerable<CustomerAddress>> GetAddressesAsync(Guid customerId);

    Task<CustomerAddress> AddAddressAsync(Guid customerId, string? label, string street, string number, string neighborhood, string city, string state, string zipCode, string? complement = null, double? latitude = null, double? longitude = null, bool isDefault = false);

    // Retorna null se o endereço não existe ou não pertence a esse customerId (nunca confiar o dono a partir do payload).
    Task<CustomerAddress?> UpdateAddressAsync(Guid customerId, Guid addressId, string? label, string street, string number, string neighborhood, string city, string state, string zipCode, string? complement = null, double? latitude = null, double? longitude = null, bool isDefault = false);
}
