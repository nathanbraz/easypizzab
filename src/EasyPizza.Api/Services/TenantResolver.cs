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

        // Extrai o slug do hostname (ex: "pizzatop" de "pizzatop.easypizza.com.br" ou "pizzatop.lvh.me")
        string? hostSlug = null;
        var host = httpContext.Request.Host.Host;
        if (!string.IsNullOrEmpty(host) && host.Contains('.'))
        {
            var parts = host.Split('.');
            if (parts[0] != "www" && parts[0] != "localhost" && parts[0] != "api" && parts[0] != "admin" && parts[0] != "master")
            {
                hostSlug = parts[0];
            }
        }

        // Tenta obter o slug do tenant a partir do header (maior prioridade), depois do host, e por fim da rota ou query
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

        // Resolve o MasterDbContext usando um novo escopo para não prendê-lo em um singleton ou dependência circular
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
        GetConnectionString(); // Isso irá resolver o tenant e a string de conexão
        return _tenant;
    }
}
