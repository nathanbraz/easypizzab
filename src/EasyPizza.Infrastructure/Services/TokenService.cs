using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EasyPizza.Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace EasyPizza.Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(string userId, string email, string name, string role, string scope, string? tenantSlug, string securityStamp, List<string>? permissions = null)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]!);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(JwtRegisteredClaimNames.Name, name),
            new Claim(ClaimTypes.Role, role),
            new Claim("Scope", scope), // "Master" ou "Tenant"
            new Claim("AspNet.Identity.SecurityStamp", securityStamp)
        };

        if (!string.IsNullOrEmpty(tenantSlug))
        {
            claims.Add(new Claim("TenantSlug", tenantSlug));
        }

        if (permissions != null && permissions.Any())
        {
            foreach (var permission in permissions)
            {
                claims.Add(new Claim("Permission", permission));
            }
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(8),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
