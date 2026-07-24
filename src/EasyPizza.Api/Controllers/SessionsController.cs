using EasyPizza.Application.DTOs;
using EasyPizza.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EasyPizza.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SessionsController : ControllerBase
{
    private readonly ISessionService _sessionService;

    public SessionsController(ISessionService sessionService)
    {
        _sessionService = sessionService;
    }

    /// <summary>
    /// Gerar um link mágico para acesso do cliente via WhatsApp.
    /// O header 'x-tenant-slug' deve ser fornecido para identificar a pizzaria.
    /// </summary>
    [HttpPost("magic-link")]
    public async Task<IActionResult> GenerateMagicLink([FromBody] GenerateSessionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.PhoneNumber))
            return BadRequest(new { success = false, message = "O número de telefone é obrigatório." });

        try
        {
            var response = await _sessionService.GenerateMagicLinkSessionAsync(request);
            
            return Ok(new
            {
                success = true,
                message = "Link mágico gerado com sucesso.",
                data = response
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    // Endpoint para consumir o magic link e iniciar sessão no front-end
    [HttpGet("{token}/customer-info")]
    public async Task<IActionResult> GetCustomerInfo(Guid token)
    {
        var info = await _sessionService.GetSessionInfoAsync(token);
        if (info == null)
            return Unauthorized(new { success = false, message = "A sessão é inválida ou expirou." });

        return Ok(new { success = true, data = info });
    }
}
