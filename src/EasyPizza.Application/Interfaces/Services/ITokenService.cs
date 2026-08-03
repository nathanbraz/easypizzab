namespace EasyPizza.Application.Interfaces.Services;

public interface ITokenService
{
    string GenerateToken(string userId, string email, string name, string role, string scope, string? tenantSlug, string securityStamp, List<string>? permissions = null);
}
