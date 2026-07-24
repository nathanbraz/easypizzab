using EasyPizza.Application.Interfaces.Repositories;
using EasyPizza.Application.Interfaces.Services;
using EasyPizza.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace EasyPizza.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SessionsController : ControllerBase
{
    private readonly IRepository<OrderSession> _sessionRepository;
    private readonly ICustomerService _customerService;

    public SessionsController(IRepository<OrderSession> sessionRepository, ICustomerService customerService)
    {
        _sessionRepository = sessionRepository;
        _customerService = customerService;
    }

    // Called by the WhatsApp Bot when a user interacts with it
    [HttpPost("{tenantSlug}/bot-webhook")]
    public async Task<IActionResult> GenerateSessionLink(string tenantSlug, [FromBody] GenerateSessionRequest request)
    {
        // 1. Get or create the customer by Phone Number
        var customer = await _customerService.GetOrCreateCustomerAsync(request.PhoneNumber, request.Name);
        
        // 2. Create a new OrderSession (valid for 2 hours)
        var session = new OrderSession(customer.Id, 2);
        await _sessionRepository.AddAsync(session);
        await _sessionRepository.SaveChangesAsync();
        
        // 3. Return the magic link data
        var magicLink = $"https://easypizza.com/{tenantSlug}/t/{session.Id}";
        
        return Ok(new GenerateSessionResponse(session.Id, magicLink, customer.Id));
    }

    // Called by the Frontend when user opens the magic link
    [HttpGet("{tenantSlug}/validate/{sessionId:guid}")]
    public async Task<IActionResult> ValidateSession(string tenantSlug, Guid sessionId)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId);
        
        if (session == null || session.ExpiresAt < DateTime.UtcNow || session.IsUsed)
        {
            return Unauthorized(new { message = "Sessão inválida, expirada ou já utilizada." });
        }
        
        // Return valid session details to the frontend
        return Ok(new ValidateSessionResponse(session.CustomerId, session.ExpiresAt));
    }
}

public record GenerateSessionRequest(string PhoneNumber, string? Name);
public record GenerateSessionResponse(Guid SessionId, string MagicLink, Guid CustomerId);
public record ValidateSessionResponse(Guid CustomerId, DateTime ExpiresAt);
