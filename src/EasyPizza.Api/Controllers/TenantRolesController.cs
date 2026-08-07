using EasyPizza.Application.DTOs.Tenant;
using EasyPizza.Domain.Constants;
using EasyPizza.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EasyPizza.Api.Controllers;

[Authorize(Policy = "RequireTenant")]
[ApiController]
[Route("api/roles")]
public class TenantRolesController : ControllerBase
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public TenantRolesController(
        RoleManager<ApplicationRole> roleManager,
        UserManager<ApplicationUser> userManager)
    {
        _roleManager = roleManager;
        _userManager = userManager;
    }

    [HttpGet("{tenantSlug}/permissions")]
    public IActionResult GetAvailablePermissions(string tenantSlug)
    {
        var permissions = Permissions.All.Select(p => new {
            Id = p,
            Name = TranslatePermission(p)
        });
        return Ok(new { success = true, data = permissions });
    }

    [HttpGet("{tenantSlug}")]
    [Authorize(Policy = Permissions.ViewRoles)]
    public async Task<IActionResult> GetAll(string tenantSlug)
    {
        var roles = await _roleManager.Roles.ToListAsync();
        var result = new List<object>();

        foreach (var role in roles)
        {
            var claims = await _roleManager.GetClaimsAsync(role);
            var permissions = claims.Where(c => c.Type == "Permission").Select(c => c.Value).ToList();
            
            result.Add(new
            {
                Id = role.Id,
                Name = role.Name,
                Permissions = permissions
            });
        }

        return Ok(new { success = true, data = result });
    }

    [HttpPost("{tenantSlug}")]
    [Authorize(Policy = Permissions.CreateRoles)]
    public async Task<IActionResult> Create(string tenantSlug, [FromBody] CreateTenantRoleRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        if (await _roleManager.RoleExistsAsync(request.Name))
        {
            return BadRequest(new { success = false, message = "Já existe um cargo com esse nome." });
        }

        var role = new ApplicationRole { Name = request.Name };
        var result = await _roleManager.CreateAsync(role);

        if (!result.Succeeded)
        {
            return BadRequest(new { success = false, message = "Erro ao criar cargo.", errors = result.Errors.Select(e => e.Description) });
        }

        foreach (var permission in request.Permissions)
        {
            await _roleManager.AddClaimAsync(role, new Claim("Permission", permission));
        }

        return Ok(new { success = true, message = "Cargo criado com sucesso.", data = new { Id = role.Id, Name = role.Name, Permissions = request.Permissions }});
    }

    [HttpPut("{tenantSlug}/{id:guid}")]
    [Authorize(Policy = Permissions.EditRoles)]
    public async Task<IActionResult> Update(string tenantSlug, Guid id, [FromBody] UpdateTenantRoleRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        var role = await _roleManager.FindByIdAsync(id.ToString());
        if (role == null) return NotFound(new { success = false, message = "Cargo não encontrado." });

        if (role.Name == "Administrador") return BadRequest(new { success = false, message = "Não é permitido alterar o cargo nativo Owner." });

        if (role.Name != request.Name)
        {
            var existing = await _roleManager.FindByNameAsync(request.Name);
            if (existing != null && existing.Id != role.Id)
            {
                return BadRequest(new { success = false, message = "Já existe outro cargo com esse nome." });
            }
            role.Name = request.Name;
            await _roleManager.UpdateAsync(role);
        }

        var existingClaims = await _roleManager.GetClaimsAsync(role);
        foreach (var claim in existingClaims.Where(c => c.Type == "Permission"))
        {
            await _roleManager.RemoveClaimAsync(role, claim);
        }

        foreach (var permission in request.Permissions)
        {
            await _roleManager.AddClaimAsync(role, new Claim("Permission", permission));
        }

        return Ok(new { success = true, message = "Cargo atualizado com sucesso." });
    }

    [HttpDelete("{tenantSlug}/{id:guid}")]
    [Authorize(Policy = Permissions.DeleteRoles)]
    public async Task<IActionResult> Delete(string tenantSlug, Guid id)
    {
        var role = await _roleManager.FindByIdAsync(id.ToString());
        if (role == null) return NotFound(new { success = false, message = "Cargo não encontrado." });

        if (role.Name == "Administrador") return BadRequest(new { success = false, message = "Não é possível excluir o cargo nativo Owner." });

        var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);
        if (usersInRole.Any())
        {
            return BadRequest(new { success = false, message = $"Existem {usersInRole.Count} usuário(s) vinculado(s) a este cargo. Altere-os antes de excluir o cargo." });
        }

        await _roleManager.DeleteAsync(role);
        return Ok(new { success = true, message = "Cargo excluído com sucesso." });
    }

    private string TranslatePermission(string permissionCode)
    {
        return permissionCode switch
        {
            Permissions.ViewOrders => "Visualizar Pedidos",
            Permissions.EditOrders => "Editar Pedidos",
            Permissions.ManageCatalog => "Gerenciar Cardápio",
            Permissions.ManageSettings => "Gerenciar Configurações",
            Permissions.ManageCoupons => "Gerenciar Cupons",
            Permissions.ManageCouriers => "Gerenciar Entregadores",
            Permissions.ViewTeam => "Visualizar Equipe",
            Permissions.CreateTeam => "Criar Equipe",
            Permissions.EditTeam => "Editar Equipe",
            Permissions.BlockTeam => "Inativar/Bloquear Equipe",
            Permissions.DeleteTeam => "Excluir Equipe",
            Permissions.ViewRoles => "Visualizar Cargos",
            Permissions.CreateRoles => "Criar Cargos",
            Permissions.EditRoles => "Editar Cargos",
            Permissions.DeleteRoles => "Excluir Cargos",
            Permissions.ViewCustomers => "Visualizar Clientes",
            _ => permissionCode
        };
    }
}
