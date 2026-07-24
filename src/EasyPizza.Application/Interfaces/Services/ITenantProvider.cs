using EasyPizza.Domain.Entities;

namespace EasyPizza.Application.Interfaces.Services;

public interface ITenantProvider
{
    string? GetConnectionString();
    Tenant? GetTenant();
}
