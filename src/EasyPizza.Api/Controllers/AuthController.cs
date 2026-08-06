using EasyPizza.Application.DTOs.Auth;
using EasyPizza.Application.Interfaces.Services;
using EasyPizza.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace EasyPizza.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<MasterUser> _masterUserManager;
    private readonly UserManager<ApplicationUser> _tenantUserManager;
    private readonly RoleManager<MasterRole> _masterRoleManager;
    private readonly RoleManager<ApplicationRole> _tenantRoleManager;
    private readonly ITokenService _tokenService;
    private readonly ITenantProvider _tenantProvider;

    public AuthController(
        UserManager<MasterUser> masterUserManager,
        UserManager<ApplicationUser> tenantUserManager,
        RoleManager<MasterRole> masterRoleManager,
        RoleManager<ApplicationRole> tenantRoleManager,
        ITokenService tokenService,
        ITenantProvider tenantProvider)
    {
        _masterUserManager = masterUserManager;
        _tenantUserManager = tenantUserManager;
        _masterRoleManager = masterRoleManager;
        _tenantRoleManager = tenantRoleManager;
        _tokenService = tokenService;
        _tenantProvider = tenantProvider;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var tenant = _tenantProvider.GetTenant();
        var tenantSlug = tenant?.Slug;

        if (string.IsNullOrEmpty(tenantSlug))
        {
            // 1. Login Exclusivo do Master (URL Global)
            var masterUser = await _masterUserManager.FindByEmailAsync(request.Email);
            if (masterUser != null)
            {
                var isMasterValid = await _masterUserManager.CheckPasswordAsync(masterUser, request.Password);
                if (isMasterValid)
                {
                    if (!masterUser.IsActive)
                    {
                        return Unauthorized(new { success = false, message = "Sua conta foi desativada pelo administrador." });
                    }

                    var roles = await _masterUserManager.GetRolesAsync(masterUser);
                    var role = roles.FirstOrDefault() ?? "Master";

                    var permissions = new List<string>();
                    foreach (var roleName in roles)
                    {
                        var mRole = await _masterRoleManager.FindByNameAsync(roleName);
                        if (mRole != null)
                        {
                            var roleClaims = await _masterRoleManager.GetClaimsAsync(mRole);
                            permissions.AddRange(roleClaims.Where(c => c.Type == "Permission").Select(c => c.Value));
                        }
                    }

                    var uniquePermissions = permissions.Distinct().ToList();
                    
                    var token = _tokenService.GenerateToken(
                        masterUser.Id.ToString(), 
                        masterUser.Email!, 
                        masterUser.Name, 
                        role, 
                        "Master", 
                        null, 
                        masterUser.SecurityStamp,
                        uniquePermissions); 

                    return Ok(new
                    {
                        success = true,
                        data = new LoginResponseDto
                        {
                            Token = token,
                            Name = masterUser.Name,
                            Email = masterUser.Email!,
                            Role = role,
                            Scope = "Master"
                        }
                    });
                }
            }
            return Unauthorized(new { success = false, message = "E-mail ou senha inválidos para o Acesso Global." });
        }
        else
        {
            // 2. Login Exclusivo de Pizzarias (URL de Lojista)
            var tenantUser = await _tenantUserManager.FindByEmailAsync(request.Email);
            if (tenantUser != null)
            {
                var isTenantValid = await _tenantUserManager.CheckPasswordAsync(tenantUser, request.Password);
                if (isTenantValid)
                {
                    var roles = await _tenantUserManager.GetRolesAsync(tenantUser);
                    var role = roles.FirstOrDefault() ?? "Operator";

                    // Extrair todas as permissões (Claims) das Roles do usuário
                    var permissions = new List<string>();
                    foreach (var roleName in roles)
                    {
                        var appRole = await _tenantRoleManager.FindByNameAsync(roleName);
                        if (appRole != null)
                        {
                            var roleClaims = await _tenantRoleManager.GetClaimsAsync(appRole);
                            permissions.AddRange(roleClaims.Where(c => c.Type == "Permission").Select(c => c.Value));
                        }
                    }

                    // Extrair permissões diretas do usuário (se houver)
                    var userClaims = await _tenantUserManager.GetClaimsAsync(tenantUser);
                    permissions.AddRange(userClaims.Where(c => c.Type == "Permission").Select(c => c.Value));

                    var uniquePermissions = permissions.Distinct().ToList();

                    var token = _tokenService.GenerateToken(
                        tenantUser.Id.ToString(), 
                        tenantUser.Email!, 
                        tenantUser.Name, 
                        role, 
                        "Tenant", 
                        tenantSlug,
                        tenantUser.SecurityStamp,
                        uniquePermissions);

                    return Ok(new
                    {
                        success = true,
                        data = new LoginResponseDto
                        {
                            Token = token,
                            Name = tenantUser.Name,
                            Email = tenantUser.Email!,
                            Role = role,
                            Scope = "Tenant"
                        }
                    });
                }
            }
            return Unauthorized(new { success = false, message = "E-mail ou senha inválidos." });
        }
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        // 1. Descobrir qual o Escopo (Master ou Tenant)
        var scope = User.FindFirst("Scope")?.Value;
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(scope))
        {
            return BadRequest(new { success = false, message = "Sessão inválida." });
        }

        // 2. Atualiza o SecurityStamp no banco, efetivamente revogando os tokens anteriores.
        if (scope == "Master")
        {
            var user = await _masterUserManager.FindByIdAsync(userId);
            if (user != null)
            {
                await _masterUserManager.UpdateSecurityStampAsync(user);
            }
        }
        else if (scope == "Tenant")
        {
            var user = await _tenantUserManager.FindByIdAsync(userId);
            if (user != null)
            {
                await _tenantUserManager.UpdateSecurityStampAsync(user);
            }
        }

        return Ok(new { success = true, message = "Logout realizado com sucesso no backend." });
    }
}
