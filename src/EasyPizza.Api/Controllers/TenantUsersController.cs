using EasyPizza.Application.DTOs.Tenant;
using EasyPizza.Domain.Constants;
using EasyPizza.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EasyPizza.Api.Controllers;

[Authorize(Policy = "RequireTenant")]
[ApiController]
[Route("api/users")]
public class TenantUsersController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public TenantUsersController(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [HttpGet("{tenantSlug}")]
    [Authorize(Policy = Permissions.ViewTeam)]
    public async Task<IActionResult> GetAll(string tenantSlug)
    {
        var users = await _userManager.Users.ToListAsync();
        var result = new List<object>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            result.Add(new
            {
                Id = user.Id,
                Name = user.Name,
                UserName = user.UserName,
                Email = user.Email,
                Role = roles.FirstOrDefault(),
                IsActive = user.IsActive
            });
        }

        return Ok(new { success = true, data = result });
    }

    [HttpPost("{tenantSlug}")]
    [Authorize(Policy = Permissions.CreateTeam)]
    public async Task<IActionResult> Create(string tenantSlug, [FromBody] CreateTenantUserRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        var existingUser = await _userManager.FindByNameAsync(request.UserName.ToLower());
        if (existingUser != null)
        {
            return BadRequest(new { success = false, message = "Já existe um usuário com este nome de usuário." });
        }

        if (!await _roleManager.RoleExistsAsync(request.RoleName))
        {
            return BadRequest(new { success = false, message = "O cargo selecionado não existe." });
        }

        var user = new ApplicationUser
        {
            UserName = request.UserName.ToLower(),
            Email = null,
            Name = request.Name,
            EmailConfirmed = true,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return BadRequest(new { success = false, message = "Erro ao criar usuário.", errors = result.Errors.Select(e => e.Description) });
        }

        await _userManager.AddToRoleAsync(user, request.RoleName);

        return Ok(new { success = true, message = "Usuário criado com sucesso." });
    }

    [HttpPut("{tenantSlug}/{id:guid}")]
    [Authorize(Policy = Permissions.EditTeam)]
    public async Task<IActionResult> Update(string tenantSlug, Guid id, [FromBody] UpdateTenantUserRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return NotFound(new { success = false, message = "Usuário não encontrado." });

        if (!await _roleManager.RoleExistsAsync(request.RoleName))
        {
            return BadRequest(new { success = false, message = "O cargo selecionado não existe." });
        }

        user.Name = request.Name;
        var updateResult = await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            return BadRequest(new { success = false, message = "Erro ao atualizar o usuário.", errors = updateResult.Errors.Select(e => e.Description) });
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        var currentRole = currentRoles.FirstOrDefault();

        // Evita remoção do último Owner
        if (currentRole == "Administrador" && request.RoleName != "Administrador")
        {
            var owners = await _userManager.GetUsersInRoleAsync("Administrador");
            if (owners.Count <= 1)
            {
                return BadRequest(new { success = false, message = "Não é possível alterar o cargo do único dono (Owner) do sistema." });
            }
        }

        if (currentRole != request.RoleName)
        {
            if (currentRole != null)
                await _userManager.RemoveFromRoleAsync(user, currentRole);
            
            await _userManager.AddToRoleAsync(user, request.RoleName);
        }

        return Ok(new { success = true, message = "Usuário atualizado com sucesso." });
    }

    [HttpPatch("{tenantSlug}/{id:guid}/toggle-status")]
    [Authorize(Policy = Permissions.BlockTeam)]
    public async Task<IActionResult> ToggleStatus(string tenantSlug, Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return NotFound(new { success = false, message = "Usuário não encontrado." });

        var roles = await _userManager.GetRolesAsync(user);
        if (roles.Contains("Administrador") && user.IsActive)
        {
            var owners = await _userManager.GetUsersInRoleAsync("Administrador");
            if (owners.Count(u => u.IsActive) <= 1)
            {
                return BadRequest(new { success = false, message = "Não é possível bloquear o único dono (Owner) ativo do sistema." });
            }
        }

        user.IsActive = !user.IsActive;
        await _userManager.UpdateAsync(user);

        if (!user.IsActive)
        {
            await _userManager.UpdateSecurityStampAsync(user);
        }

        var statusName = user.IsActive ? "desbloqueado" : "bloqueado";
        return Ok(new { success = true, message = $"Usuário {statusName} com sucesso.", isActive = user.IsActive });
    }

    [HttpDelete("{tenantSlug}/{id:guid}")]
    [Authorize(Policy = Permissions.DeleteTeam)]
    public async Task<IActionResult> Delete(string tenantSlug, Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return NotFound(new { success = false, message = "Usuário não encontrado." });

        var roles = await _userManager.GetRolesAsync(user);
        if (roles.Contains("Administrador"))
        {
            var owners = await _userManager.GetUsersInRoleAsync("Administrador");
            if (owners.Count <= 1)
            {
                return BadRequest(new { success = false, message = "Não é possível excluir o único dono (Owner) do sistema." });
            }
        }

        await _userManager.DeleteAsync(user);
        return Ok(new { success = true, message = "Usuário excluído com sucesso." });
    }
}
