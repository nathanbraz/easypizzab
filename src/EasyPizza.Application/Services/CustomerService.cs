using EasyPizza.Application.Interfaces.Repositories;
using EasyPizza.Application.Interfaces.Services;
using EasyPizza.Domain.Entities;

namespace EasyPizza.Application.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _repository;

    public CustomerService(ICustomerRepository repository)
    {
        _repository = repository;
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

    public async Task<CustomerAddress?> UpdateCustomerAddressAsync(Guid customerId, string street, string number, string neighborhood, string city, string state, string zipCode, string? complement = null)
    {
        var customer = await _repository.GetByIdAsync(customerId);
        if (customer != null)
        {
            var address = new CustomerAddress(customerId, street, number, neighborhood, city, state, zipCode, complement, true);
            customer.Addresses.Add(address);
            await _repository.SaveChangesAsync();
            return address;
        }
        return null;
    }
}
