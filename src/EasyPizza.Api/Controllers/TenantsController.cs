using EasyPizza.Domain.Constants;
using EasyPizza.Domain.Entities;
using EasyPizza.Infrastructure.Data;
using EasyPizza.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Authorization;

namespace EasyPizza.Api.Controllers;

[Authorize(Policy = "RequireMaster")]
[ApiController]
[Route("api/master/[controller]")]
public class TenantsController : ControllerBase
{
    private readonly MasterDbContext _masterDb;
    private readonly IConfiguration _configuration;

    public TenantsController(MasterDbContext masterDb, IConfiguration configuration)
    {
        _masterDb = masterDb;
        _configuration = configuration;
    }

    [Authorize(Policy = MasterPermissions.ViewTenants)]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var tenants = await _masterDb.Tenants.OrderByDescending(t => t.CreatedAt).ToListAsync();
        return Ok(tenants);
    }

    [Authorize(Policy = MasterPermissions.CreateTenants)]
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

        // Template configurável por ambiente (env var Tenant__DefaultConnectionStringTemplate em
        // produção/homologação; em dev local cai no fallback abaixo, que aponta pro Postgres do
        // docker-compose) — assim cadastrar uma loja nova nunca exige preencher o campo de
        // ConnectionString manualmente. Ele continua existindo só como override avançado.
        var connectionStringTemplate = _configuration["Tenant:DefaultConnectionStringTemplate"]
            ?? "Host=db;Port=5432;Database=easypizza_{slug};Username=postgres;Password=1234";

        var connectionString = !string.IsNullOrWhiteSpace(request.ConnectionString)
            ? request.ConnectionString
            : connectionStringTemplate.Replace("{slug}", slug);

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

            await EnsureAdminUserAsync(tenantDbContext);
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

    // Garante que o tenant tem pelo menos um usuário com o papel "Administrador".
    // Idempotente (não cria duplicado se já existir) — usado na criação do tenant e nas rotas de
    // sincronização, pra tenants mais antigos (criados antes dessa auto-criação existir, ou cuja
    // criação falhou parcialmente) também ficarem com acesso.
    private static async Task EnsureAdminUserAsync(EasyPizzaDbContext tenantDbContext)
    {
        var adminRole = await tenantDbContext.Roles.FirstOrDefaultAsync(r => r.Name == "Administrador");
        if (adminRole == null)
            return;

        var hasAdminUser = await tenantDbContext.UserRoles.AnyAsync(ur => ur.RoleId == adminRole.Id);
        if (hasAdminUser)
            return;

        var passwordHasher = new Microsoft.AspNetCore.Identity.PasswordHasher<ApplicationUser>();
        var adminUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "admin",
            NormalizedUserName = "ADMIN",
            Email = null,
            EmailConfirmed = true,
            Name = "Administrador",
            IsActive = true,
            SecurityStamp = Guid.NewGuid().ToString()
        };

        adminUser.PasswordHash = passwordHasher.HashPassword(adminUser, "Admin@123");
        tenantDbContext.Users.Add(adminUser);

        tenantDbContext.UserRoles.Add(new Microsoft.AspNetCore.Identity.IdentityUserRole<Guid>
        {
            UserId = adminUser.Id,
            RoleId = adminRole.Id
        });

        await tenantDbContext.SaveChangesAsync();
    }

    // Só o Master define isso — é ele quem cria a instância no Evolution API (Manager UI) e
    // recebe o token dela. O lojista nunca vê nem edita esse campo (ver SettingsController,
    // que não aceita mais WhatsappApiKey no PUT do admin da loja).
    [Authorize(Policy = MasterPermissions.EditTenants)]
    [HttpPut("{slug}/whatsapp-api-key")]
    public async Task<IActionResult> SetWhatsappApiKey(string slug, [FromBody] SetWhatsappApiKeyRequest request)
    {
        var tenant = await _masterDb.Tenants.FirstOrDefaultAsync(t => t.Slug == slug.ToLower());
        if (tenant == null) return NotFound("Tenant não encontrado no Banco Mestre.");

        try
        {
            var optionsBuilder = new DbContextOptionsBuilder<EasyPizzaDbContext>();
            optionsBuilder.UseNpgsql(tenant.ConnectionString);
            using var tenantDbContext = new EasyPizzaDbContext(optionsBuilder.Options);

            var settingsRepo = new StoreSettingsRepository(tenantDbContext);
            var settings = await settingsRepo.GetSettingsAsync();
            settings.SetWhatsappApiKey(request.WhatsappApiKey);
            await settingsRepo.UpdateAsync(settings);

            return Ok(new { success = true, message = $"WhatsappApiKey de {tenant.Name} ({tenant.Slug}) atualizado." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Erro ao atualizar WhatsappApiKey: " + ex.Message });
        }
    }

    // Idem WhatsappApiKey: só o Master sabe se a credencial de pagamento colada pelo lojista é
    // de teste ou de produção (ele quem orientou o lojista a gerar o token, ou configurou na
    // hora do onboarding) — por isso esse campo saiu da tela de Configurações da loja.
    [Authorize(Policy = MasterPermissions.ViewTenants)]
    [HttpGet("{slug}/payment-sandbox-mode")]
    public async Task<IActionResult> GetPaymentSandboxMode(string slug)
    {
        var tenant = await _masterDb.Tenants.FirstOrDefaultAsync(t => t.Slug == slug.ToLower());
        if (tenant == null) return NotFound("Tenant não encontrado no Banco Mestre.");

        try
        {
            var optionsBuilder = new DbContextOptionsBuilder<EasyPizzaDbContext>();
            optionsBuilder.UseNpgsql(tenant.ConnectionString);
            using var tenantDbContext = new EasyPizzaDbContext(optionsBuilder.Options);

            var settingsRepo = new StoreSettingsRepository(tenantDbContext);
            var settings = await settingsRepo.GetSettingsAsync();

            return Ok(new { paymentGatewaySandboxMode = settings.PaymentGatewaySandboxMode });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Erro ao consultar o ambiente de pagamento: " + ex.Message });
        }
    }

    [Authorize(Policy = MasterPermissions.EditTenants)]
    [HttpPut("{slug}/payment-sandbox-mode")]
    public async Task<IActionResult> SetPaymentSandboxMode(string slug, [FromBody] SetPaymentSandboxModeRequest request)
    {
        var tenant = await _masterDb.Tenants.FirstOrDefaultAsync(t => t.Slug == slug.ToLower());
        if (tenant == null) return NotFound("Tenant não encontrado no Banco Mestre.");

        try
        {
            var optionsBuilder = new DbContextOptionsBuilder<EasyPizzaDbContext>();
            optionsBuilder.UseNpgsql(tenant.ConnectionString);
            using var tenantDbContext = new EasyPizzaDbContext(optionsBuilder.Options);

            var settingsRepo = new StoreSettingsRepository(tenantDbContext);
            var settings = await settingsRepo.GetSettingsAsync();
            settings.SetPaymentGatewaySandboxMode(request.PaymentGatewaySandboxMode);
            await settingsRepo.UpdateAsync(settings);

            return Ok(new { success = true, message = $"Ambiente de pagamento de {tenant.Name} ({tenant.Slug}) atualizado." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Erro ao atualizar o ambiente de pagamento: " + ex.Message });
        }
    }

    [Authorize(Policy = MasterPermissions.EditTenants)]
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

            await EnsureAdminUserAsync(tenantDbContext);

            return Ok(new { success = true, message = $"Banco de dados de {tenant.Name} ({tenant.Slug}) migrado com sucesso." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Erro ao migrar banco do tenant: " + ex.Message });
        }
    }

    [Authorize(Policy = MasterPermissions.EditTenants)]
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

                await EnsureAdminUserAsync(tenantDbContext);

                results.Add(new { slug = tenant.Slug, status = "Success" });
            }
            catch (Exception ex)
            {
                results.Add(new { slug = tenant.Slug, status = "Failed", error = ex.Message });
            }
        }

        return Ok(new { success = true, message = "Sincronização em massa concluída.", details = results });
    }

    [Authorize(Policy = MasterPermissions.BlockTenants)]
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

public record CreateTenantRequest(
    string Name,
    string? Slug,
    string? ConnectionString);

public record SetWhatsappApiKeyRequest(string? WhatsappApiKey);
public record SetPaymentSandboxModeRequest(bool PaymentGatewaySandboxMode);

