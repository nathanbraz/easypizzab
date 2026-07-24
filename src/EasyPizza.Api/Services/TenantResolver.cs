using EasyPizza.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EasyPizza.Api.Services;

public interface ITenantProvider
{
    string? GetConnectionString();
}

public class HttpTenantProvider : ITenantProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IServiceProvider _serviceProvider;
    private string? _connectionString;

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

        // Try to get the tenant slug from the route, header, or query
        var tenantSlug = httpContext.Request.RouteValues["tenantSlug"]?.ToString() 
                         ?? httpContext.Request.Headers["X-Tenant-Slug"].ToString()
                         ?? httpContext.Request.Query["tenantSlug"].ToString();

        if (string.IsNullOrEmpty(tenantSlug))
            return null;

        // Resolve MasterDbContext using a new scope so we don't trap it in a singleton or circular dependency
        using var scope = _serviceProvider.CreateScope();
        var masterDbContext = scope.ServiceProvider.GetRequiredService<MasterDbContext>();
        
        var tenant = masterDbContext.Tenants.AsNoTracking().FirstOrDefault(t => t.Slug == tenantSlug.ToLower());
        
        if (tenant != null)
        {
            _connectionString = tenant.ConnectionString;
        }

        return _connectionString;
    }
}
