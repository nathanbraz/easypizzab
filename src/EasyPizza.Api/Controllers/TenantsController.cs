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
            ? request.Name.ToLower().Replace(" ", "") 
            : request.Slug.ToLower();

        var exists = await _masterDb.Tenants.AnyAsync(t => t.Slug == slug);
        if (exists) return BadRequest("Tenant slug already exists.");

        var tenant = new Tenant(request.Name, slug, request.ConnectionString);
        _masterDb.Tenants.Add(tenant);
        await _masterDb.SaveChangesAsync();

        return Ok(tenant);
    }
}

public record CreateTenantRequest(string Name, string Slug, string ConnectionString);
