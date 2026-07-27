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

    public async Task<CustomerAddress?> UpdateCustomerAddressAsync(Guid customerId, string street, string number, string neighborhood, string city, string state, string zipCode, string? complement = null, double? latitude = null, double? longitude = null)
    {
        var customer = await _repository.GetByIdAsync(customerId);
        if (customer != null)
        {
            var existingAddress = customer.Addresses.FirstOrDefault();
            if (existingAddress != null)
            {
                existingAddress.UpdateAddress(street, number, neighborhood, city, state, zipCode, complement, latitude, longitude);
                await _addressRepository.UpdateAsync(existingAddress);
            }
            else
            {
                existingAddress = new CustomerAddress(customerId, street, number, neighborhood, city, state, zipCode, complement, true, latitude, longitude);
                await _addressRepository.AddAsync(existingAddress);
            }
            await _repository.SaveChangesAsync();
            return existingAddress;
        }
        return null;
    }
}
