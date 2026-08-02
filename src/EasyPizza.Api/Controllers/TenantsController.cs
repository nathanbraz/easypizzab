using EasyPizza.Domain.Entities;
using EasyPizza.Infrastructure.Data;
using EasyPizza.Infrastructure.Repositories;
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
        var tenants = await _masterDb.Tenants.OrderByDescending(t => t.CreatedAt).ToListAsync();
        return Ok(tenants);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTenantRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("O nome do tenant é obrigatório.");

        var slug = !string.IsNullOrWhiteSpace(request.Slug) 
            ? request.Slug.ToLower().Trim() 
            : request.Name.ToLower().Replace(" ", "").Replace("-", "").Trim();

        var existing = await _masterDb.Tenants.FirstOrDefaultAsync(t => t.Slug == slug);
        if (existing != null)
            return BadRequest($"Já existe uma empresa cadastrada com o subdomínio/identificador '{slug}'.");

        var connectionString = !string.IsNullOrWhiteSpace(request.ConnectionString)
            ? request.ConnectionString
            : $"Host=db;Port=5432;Database=easypizza_{slug};Username=postgres;Password=1234";

        var tenant = new Tenant(request.Name, slug, connectionString);
        _masterDb.Tenants.Add(tenant);
        await _masterDb.SaveChangesAsync();

        try
        {
            var optionsBuilder = new DbContextOptionsBuilder<EasyPizzaDbContext>();
            optionsBuilder.UseNpgsql(connectionString);
            using var tenantDbContext = new EasyPizzaDbContext(optionsBuilder.Options);
            await tenantDbContext.Database.MigrateAsync();

            var settingsRepo = new StoreSettingsRepository(tenantDbContext);
            await settingsRepo.GetSettingsAsync();
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

            var settingsRepo = new StoreSettingsRepository(tenantDbContext);
            await settingsRepo.GetSettingsAsync();

            return Ok(new { success = true, message = $"Banco de dados de {tenant.Name} ({tenant.Slug}) migrado com sucesso." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Erro ao migrar banco do tenant: " + ex.Message });
        }
    }

    [HttpPost("sync-all")]
    public async Task<IActionResult> SyncAllTenants()
    {
        var tenants = await _masterDb.Tenants.ToListAsync();
        var results = new List<object>();

        foreach (var tenant in tenants)
        {
            try
            {
                var optionsBuilder = new DbContextOptionsBuilder<EasyPizzaDbContext>();
                optionsBuilder.UseNpgsql(tenant.ConnectionString);
                using var tenantDbContext = new EasyPizzaDbContext(optionsBuilder.Options);
                await tenantDbContext.Database.MigrateAsync();
                
                var settingsRepo = new StoreSettingsRepository(tenantDbContext);
                await settingsRepo.GetSettingsAsync();

                results.Add(new { slug = tenant.Slug, status = "Success" });
            }
            catch (Exception ex)
            {
                results.Add(new { slug = tenant.Slug, status = "Failed", error = ex.Message });
            }
        }

        return Ok(new { success = true, message = "Sincronização em massa concluída.", details = results });
    }

    [HttpPut("{slug}/toggle-status")]
    public async Task<IActionResult> ToggleStatus(string slug)
    {
        var tenant = await _masterDb.Tenants.FirstOrDefaultAsync(t => t.Slug == slug.ToLower());
        if (tenant == null) return NotFound("Tenant não encontrado no Banco Mestre.");

        if (tenant.IsActive)
        {
            tenant.Deactivate();
        }
        else
        {
            tenant.Activate();
        }

        await _masterDb.SaveChangesAsync();
        return Ok(new { success = true, tenant });
    }
}

public record CreateTenantRequest(string Name, string? Slug, string? ConnectionString);
