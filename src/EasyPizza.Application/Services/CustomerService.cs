using EasyPizza.Application.Interfaces.Repositories;
using EasyPizza.Application.Interfaces.Services;
using EasyPizza.Domain.Entities;

namespace EasyPizza.Application.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _repository;
    private readonly IRepository<CustomerAddress> _addressRepository;

    public CustomerService(ICustomerRepository repository, IRepository<CustomerAddress> addressRepository)
    {
        _repository = repository;
        _addressRepository = addressRepository;
    }

    public async Task<Customer> GetOrCreateCustomerAsync(string phoneNumber, string? name = null)
    {
        var customer = await _repository.GetByPhoneNumberAsync(phoneNumber);
        if (customer == null)
        {
            customer = new Customer(phoneNumber, name);
            await _repository.AddAsync(customer);
            await _repository.SaveChangesAsync();
        }
        else if (!string.IsNullOrEmpty(name) && customer.Name != name)
        {
            customer.UpdateName(name);
            await _repository.SaveChangesAsync();
        }

        return customer;
    }

    public async Task<IEnumerable<CustomerAddress>> GetAddressesAsync(Guid customerId)
    {
        var customer = await _repository.GetByIdAsync(customerId);
        if (customer == null)
            return Enumerable.Empty<CustomerAddress>();

        return customer.Addresses
            .OrderByDescending(a => a.IsDefault)
            .ThenByDescending(a => a.CreatedAt)
            .ToList();
    }

    public async Task<CustomerAddress> AddAddressAsync(Guid customerId, string? label, string street, string number, string neighborhood, string city, string state, string zipCode, string? complement = null, double? latitude = null, double? longitude = null, bool isDefault = false)
    {
        var customer = await _repository.GetByIdAsync(customerId)
            ?? throw new InvalidOperationException("Cliente não encontrado.");

        // O primeiro endereço do cliente vira padrão automaticamente, independente do que foi pedido.
        var makeDefault = isDefault || customer.Addresses.Count == 0;

        if (makeDefault)
        {
            foreach (var existing in customer.Addresses.Where(a => a.IsDefault))
            {
                existing.RemoveDefault();
                await _addressRepository.UpdateAsync(existing);
            }
        }

        var address = new CustomerAddress(customerId, street, number, neighborhood, city, state, zipCode, complement, makeDefault, latitude, longitude, label);
        await _addressRepository.AddAsync(address);
        await _repository.SaveChangesAsync();

        return address;
    }

    public async Task<CustomerAddress?> UpdateAddressAsync(Guid customerId, Guid addressId, string? label, string street, string number, string neighborhood, string city, string state, string zipCode, string? complement = null, double? latitude = null, double? longitude = null, bool isDefault = false)
    {
        var customer = await _repository.GetByIdAsync(customerId);
        var address = customer?.Addresses.FirstOrDefault(a => a.Id == addressId);
        if (customer == null || address == null)
            return null;

        if (isDefault && !address.IsDefault)
        {
            foreach (var existing in customer.Addresses.Where(a => a.IsDefault && a.Id != addressId))
            {
                existing.RemoveDefault();
                await _addressRepository.UpdateAsync(existing);
            }
            address.SetAsDefault();
        }

        address.UpdateAddress(street, number, neighborhood, city, state, zipCode, complement, latitude, longitude, label);
        await _addressRepository.UpdateAsync(address);
        await _repository.SaveChangesAsync();

        return address;
    }
}
