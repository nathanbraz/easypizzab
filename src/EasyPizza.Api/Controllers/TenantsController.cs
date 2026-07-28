using EasyPizza.Domain.Entities;
using EasyPizza.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EasyPizza.Api.Controllers;

[ApiController]
[Route("api/superadmin/[controller]")]
public class TenantsController : ControllerBase
{
    private readonly MasterDbContext _masterDb;

    public TenantsController(MasterDbContext masterDb)
    {
        _masterDb = masterDb;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var tenants = await _masterDb.Tenants.ToListAsync();
        return Ok(tenants);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTenantRequest request)
    {
        // Generates slug from name if not provided
        var slug = string.IsNullOrWhiteSpace(request.Slug) 
            ? request.Name.ToLower().Replace(" ", "").Replace("-", "") 
            : request.Slug.ToLower();

        var exists = await _masterDb.Tenants.AnyAsync(t => t.Slug == slug);
        if (exists) return BadRequest("O slug deste tenant já existe.");

        var connectionString = string.IsNullOrWhiteSpace(request.ConnectionString)
            ? $"Host=localhost;Database=easypizza_{slug};Username=postgres;Password=1234"
            : request.ConnectionString;

        var tenant = new Tenant(request.Name, slug, connectionString);
        _masterDb.Tenants.Add(tenant);
        await _masterDb.SaveChangesAsync();

        try
        {
            var optionsBuilder = new DbContextOptionsBuilder<EasyPizzaDbContext>();
            optionsBuilder.UseNpgsql(connectionString);
            using var tenantDbContext = new EasyPizzaDbContext(optionsBuilder.Options);
            await tenantDbContext.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            return Ok(new { 
                tenant, 
                migrationWarning = "Tenant salvo no Banco Mestre, mas ocorreu um erro na migração automática do banco físico: " + ex.Message 
            });
        }

        return Ok(tenant);
    }

    [HttpPost("{slug}/migrate")]
    public async Task<IActionResult> MigrateTenant(string slug)
    {
        var tenant = await _masterDb.Tenants.FirstOrDefaultAsync(t => t.Slug == slug.ToLower());
        if (tenant == null) return NotFound("Tenant não encontrado no Banco Mestre.");

        try
        {
            var optionsBuilder = new DbContextOptionsBuilder<EasyPizzaDbContext>();
            optionsBuilder.UseNpgsql(tenant.ConnectionString);
            using var tenantDbContext = new EasyPizzaDbContext(optionsBuilder.Options);
            await tenantDbContext.Database.MigrateAsync();
            return Ok(new { success = true, message = $"Banco de dados de {tenant.Name} ({tenant.Slug}) migrado com sucesso." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Erro ao migrar banco do tenant: " + ex.Message });
        }
    }
}

public record CreateTenantRequest(string Name, string? Slug, string? ConnectionString);
