using EasyPizza.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace EasyPizza.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    // Called automatically by Webhook/Bot when someone sends a message
    [HttpPost("{tenantSlug}/register")]
    public async Task<IActionResult> RegisterFromWhatsApp(string tenantSlug, [FromBody] RegisterCustomerRequest request)
    {
        var customer = await _customerService.GetOrCreateCustomerAsync(request.PhoneNumber, request.Name);
        return Ok(customer);
    }

    // Called by Frontend when user is at checkout
    [HttpPut("{tenantSlug}/{id:guid}/address")]
    public async Task<IActionResult> UpdateAddress(string tenantSlug, Guid id, [FromBody] UpdateAddressRequest request)
    {
        var address = await _customerService.UpdateCustomerAddressAsync(
            id, request.Street, request.Number, request.Neighborhood, 
            request.City, request.State, request.ZipCode, request.Complement);
            
        return Ok(address);
    }
}

public record RegisterCustomerRequest(string PhoneNumber, string? Name);
public record UpdateAddressRequest(string Street, string Number, string Neighborhood, string City, string State, string ZipCode, string? Complement);
