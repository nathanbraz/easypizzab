using EasyPizza.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EasyPizza.Api.Services;

using EasyPizza.Application.Interfaces.Services;

public class HttpTenantProvider : ITenantProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IServiceProvider _serviceProvider;
    private string? _connectionString;
    private EasyPizza.Domain.Entities.Tenant? _tenant;

    public HttpTenantProvider(IHttpContextAccessor httpContextAccessor, IServiceProvider serviceProvider)
    {
        _httpContextAccessor = httpContextAccessor;
        _serviceProvider = serviceProvider;
    }

    public string? GetConnectionString()
    {
        if (_connectionString != null)
            return _connectionString;

        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null) return null;

        // Extract hostname slug (e.g. "pizzatop" from "pizzatop.easypizza.com.br" or "pizzatop.lvh.me")
        string? hostSlug = null;
        var host = httpContext.Request.Host.Host;
        if (!string.IsNullOrEmpty(host) && host.Contains('.'))
        {
            var parts = host.Split('.');
            if (parts[0] != "www" && parts[0] != "localhost" && parts[0] != "api" && parts[0] != "admin" && parts[0] != "superadmin")
            {
                hostSlug = parts[0];
            }
        }

        // Try to get the tenant slug from header (highest priority), then host, then route or query
        var headerSlug = httpContext.Request.Headers["X-Tenant-Slug"].ToString();
        var routeSlug = httpContext.Request.RouteValues["tenantSlug"]?.ToString();
        var instanceSlug = httpContext.Request.RouteValues["instanceName"]?.ToString();
        var querySlug = httpContext.Request.Query["tenantSlug"].ToString();

        var tenantSlug = !string.IsNullOrWhiteSpace(headerSlug) ? headerSlug :
                         !string.IsNullOrWhiteSpace(hostSlug) ? hostSlug :
                         !string.IsNullOrWhiteSpace(routeSlug) ? routeSlug :
                         !string.IsNullOrWhiteSpace(instanceSlug) ? instanceSlug :
                         !string.IsNullOrWhiteSpace(querySlug) ? querySlug : null;

        if (string.IsNullOrEmpty(tenantSlug))
            return null;

        // Resolve MasterDbContext using a new scope so we don't trap it in a singleton or circular dependency
        using var scope = _serviceProvider.CreateScope();
        var masterDbContext = scope.ServiceProvider.GetRequiredService<MasterDbContext>();
        
        var tenant = masterDbContext.Tenants.AsNoTracking().FirstOrDefault(t => t.Slug == tenantSlug.ToLower());
        
        if (tenant != null)
        {
            _tenant = tenant;
            _connectionString = tenant.ConnectionString;
        }

        return _connectionString;
    }

    public EasyPizza.Domain.Entities.Tenant? GetTenant()
    {
        if (_tenant != null) return _tenant;
        GetConnectionString(); // This will resolve the tenant and connection string
        return _tenant;
    }
}
