using EasyPizza.Domain.Entities;
using EasyPizza.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EasyPizza.Api.Controllers;

[ApiController]
[Route("api/superadmin/dashboard")]
public class SuperAdminDashboardController : ControllerBase
{
    private readonly MasterDbContext _masterDb;

    public SuperAdminDashboardController(MasterDbContext masterDb)
    {
        _masterDb = masterDb;
    }

    [HttpGet]
    public async Task<IActionResult> GetDashboardMetrics()
    {
        var tenants = await _masterDb.Tenants.ToListAsync();
        
        var totalTenants = tenants.Count;
        var activeTenants = tenants.Count(t => t.IsActive);
        var suspendedTenants = tenants.Count(t => !t.IsActive);

        // Num SaaS real, poderiamos consultar de forma cross-database os pedidos para somar o faturamento.
        // Como o EF Core não suporta cross-database facilmente sem queries complexas no Postgres,
        // retornaremos métricas mockadas ou apenas do Master DB por enquanto.

        return Ok(new
        {
            success = true,
            totalTenants,
            activeTenants,
            suspendedTenants,
            systemStatus = "Online"
        });
    }

    [HttpGet("settings")]
    public async Task<IActionResult> GetSettings()
    {
        var settings = await _masterDb.GlobalSettings.FirstOrDefaultAsync();
        if (settings == null)
        {
            return Ok(new { success = true, data = new { globalAnnouncementMessage = "", isAnnouncementActive = false } });
        }

        return Ok(new { success = true, data = settings });
    }

    [HttpPut("settings")]
    public async Task<IActionResult> UpdateSettings([FromBody] UpdateGlobalSettingsRequest request)
    {
        var settings = await _masterDb.GlobalSettings.FirstOrDefaultAsync();
        
        if (settings == null)
        {
            settings = new GlobalSaaSSettings();
            settings.UpdateAnnouncement(request.GlobalAnnouncementMessage, request.IsAnnouncementActive);
            _masterDb.GlobalSettings.Add(settings);
        }
        else
        {
            settings.UpdateAnnouncement(request.GlobalAnnouncementMessage, request.IsAnnouncementActive);
        }

        await _masterDb.SaveChangesAsync();
        return Ok(new { success = true, data = settings });
    }
}

public record UpdateGlobalSettingsRequest(string? GlobalAnnouncementMessage, bool IsAnnouncementActive);
