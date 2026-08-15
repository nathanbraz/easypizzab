using EasyPizza.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace EasyPizza.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly ICurrentCustomerAccessor _currentCustomer;

    public CustomersController(ICustomerService customerService, ICurrentCustomerAccessor currentCustomer)
    {
        _customerService = customerService;
        _currentCustomer = currentCustomer;
    }

    // Chamado automaticamente pelo Webhook/Bot quando alguém envia uma mensagem.
    // Anônimo de propósito: é o próprio ponto de entrada do cadastro implícito (ainda não existe sessão).
    [HttpPost("{tenantSlug}/register")]
    public async Task<IActionResult> RegisterFromWhatsApp(string tenantSlug, [FromBody] RegisterCustomerRequest request)
    {
        var customer = await _customerService.GetOrCreateCustomerAsync(request.PhoneNumber, request.Name);
        return Ok(customer);
    }

    // Lista os endereços salvos do cliente da sessão atual (endereço padrão primeiro).
    [Authorize(Policy = "RequireCustomerSession")]
    [HttpGet("{tenantSlug}/addresses")]
    public async Task<IActionResult> GetAddresses(string tenantSlug)
    {
        var customerId = _currentCustomer.CustomerId!.Value;
        var addresses = await _customerService.GetAddressesAsync(customerId);
        return Ok(addresses);
    }

    // Cadastra um novo endereço pro cliente da sessão atual, sem apagar os que ele já tinha.
    [Authorize(Policy = "RequireCustomerSession")]
    [HttpPost("{tenantSlug}/addresses")]
    public async Task<IActionResult> AddAddress(string tenantSlug, [FromBody] AddressRequest request)
    {
        var customerId = _currentCustomer.CustomerId!.Value;
        var address = await _customerService.AddAddressAsync(
            customerId, request.Label, request.Street, request.Number, request.Neighborhood,
            request.City, request.State, request.ZipCode, request.Complement, request.Latitude, request.Longitude, request.IsDefault);

        return Ok(address);
    }

    // Edita um endereço já existente do cliente da sessão atual (a posse é validada no service).
    [Authorize(Policy = "RequireCustomerSession")]
    [HttpPut("{tenantSlug}/addresses/{addressId:guid}")]
    public async Task<IActionResult> UpdateAddress(string tenantSlug, Guid addressId, [FromBody] AddressRequest request)
    {
        var customerId = _currentCustomer.CustomerId!.Value;
        var address = await _customerService.UpdateAddressAsync(
            customerId, addressId, request.Label, request.Street, request.Number, request.Neighborhood,
            request.City, request.State, request.ZipCode, request.Complement, request.Latitude, request.Longitude, request.IsDefault);

        if (address == null)
            return NotFound(new { success = false, message = "Endereço não encontrado." });

        return Ok(address);
    }
}

public record RegisterCustomerRequest(string PhoneNumber, string? Name);
public record AddressRequest(string Street, string Number, string Neighborhood, string City, string State, string ZipCode, string? Complement = null, double? Latitude = null, double? Longitude = null, string? Label = null, bool IsDefault = false);
