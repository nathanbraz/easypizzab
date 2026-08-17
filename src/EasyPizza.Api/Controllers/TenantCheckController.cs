using EasyPizza.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EasyPizza.Api.Controllers;

// Endpoint público (sem autenticação, de propósito) consultado pelo Kamal Proxy antes de
// emitir certificado TLS "on-demand" para um subdomínio novo (ver config/deploy.yml do
// easypizza — proxy.tls_on_demand). Só confirma se o subdomínio pedido corresponde a um
// tenant real e ativo; não expõe nenhum dado do tenant, só 200/404.
[ApiController]
[Route("api/tenant-check")]
public class TenantCheckController : ControllerBase
{
    // Mesma lista de segmentos reservados usada em HttpTenantProvider, pra nunca tratar
    // um desses como se fosse slug de loja.
    private static readonly HashSet<string> ReservedHostSegments = new(StringComparer.OrdinalIgnoreCase)
    {
        "www", "localhost", "api", "admin", "master"
    };

    private readonly MasterDbContext _masterDb;

    public TenantCheckController(MasterDbContext masterDb)
    {
        _masterDb = masterDb;
    }

    [HttpGet]
    public async Task<IActionResult> Check([FromQuery] string? host)
    {
        if (string.IsNullOrWhiteSpace(host))
            return BadRequest();

        var slug = host.Split('.')[0].Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(slug) || ReservedHostSegments.Contains(slug))
            return NotFound();

        var exists = await _masterDb.Tenants.AnyAsync(t => t.Slug == slug && t.IsActive);
        return exists ? Ok() : NotFound();
    }
}
