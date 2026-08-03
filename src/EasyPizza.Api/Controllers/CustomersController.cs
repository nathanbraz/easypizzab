using EasyPizza.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace EasyPizza.Api.Controllers;

[Authorize(Policy = "RequireTenant")]
[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    // Chamado automaticamente pelo Webhook/Bot quando alguém envia uma mensagem
    [HttpPost("{tenantSlug}/register")]
    public async Task<IActionResult> RegisterFromWhatsApp(string tenantSlug, [FromBody] RegisterCustomerRequest request)
    {
        var customer = await _customerService.GetOrCreateCustomerAsync(request.PhoneNumber, request.Name);
        return Ok(customer);
    }

    // Chamado pelo Frontend quando o usuário está no checkout
    [HttpPut("{tenantSlug}/{id:guid}/address")]
    public async Task<IActionResult> UpdateAddress(string tenantSlug, Guid id, [FromBody] UpdateAddressRequest request)
    {
        var address = await _customerService.UpdateCustomerAddressAsync(
            id, request.Street, request.Number, request.Neighborhood, 
            request.City, request.State, request.ZipCode, request.Complement, request.Latitude, request.Longitude);
            
        return Ok(address);
    }
}

public record RegisterCustomerRequest(string PhoneNumber, string? Name);
public record UpdateAddressRequest(string Street, string Number, string Neighborhood, string City, string State, string ZipCode, string? Complement, double? Latitude, double? Longitude);
