using EasyPizza.Application.DTOs.Master;
using EasyPizza.Domain.Constants;
using EasyPizza.Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EasyPizza.Api.Controllers;

[Authorize(Policy = "RequireMaster")]
[ApiController]
[Route("api/master/users")]
public class MasterUsersController : ControllerBase
{
    private readonly UserManager<MasterUser> _userManager;
    private readonly RoleManager<MasterRole> _roleManager;
    private readonly IValidator<CreateMasterUserRequestDto> _createValidator;

    public MasterUsersController(
        UserManager<MasterUser> userManager,
        RoleManager<MasterRole> roleManager,
        IValidator<CreateMasterUserRequestDto> createValidator)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _createValidator = createValidator;
    }

    [HttpGet]
    [Authorize(Policy = MasterPermissions.ViewMasterTeam)]
    public async Task<IActionResult> GetAll()
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
                Email = user.Email,
                Role = roles.FirstOrDefault(),
                IsActive = user.IsActive
            });
        }

        return Ok(new { success = true, data = result });
    }

    [HttpPost]
    [Authorize(Policy = MasterPermissions.CreateMasterTeam)]
    public async Task<IActionResult> Create([FromBody] CreateMasterUserRequestDto request)
    {
        var validationResult = await _createValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { success = false, errors = validationResult.Errors.Select(e => e.ErrorMessage) });
        }

        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return BadRequest(new { success = false, message = "Já existe um usuário com este e-mail." });
        }

        if (!await _roleManager.RoleExistsAsync(request.RoleName))
        {
            return BadRequest(new { success = false, message = "O cargo selecionado não existe." });
        }

        var user = new MasterUser
        {
            UserName = request.Email,
            Email = request.Email,
            Name = request.Name,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return BadRequest(new { success = false, message = "Erro ao criar usuário.", errors = result.Errors.Select(e => e.Description) });
        }

        await _userManager.AddToRoleAsync(user, request.RoleName);

        return Ok(new { success = true, message = "Usuário criado com sucesso." });
    }

    [HttpPut("{id}")]
    [Authorize(Policy = MasterPermissions.EditMasterTeam)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMasterUserRequestDto request)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return NotFound(new { success = false, message = "Usuário não encontrado." });

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { success = false, message = "O nome é obrigatório." });
        }

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

        // Handle Role Update
        var currentRoles = await _userManager.GetRolesAsync(user);
        var currentRole = currentRoles.FirstOrDefault();

        // Evita remoção do último Master
        if (currentRole == "Master" && request.RoleName != "Master")
        {
            var masterUsers = await _userManager.GetUsersInRoleAsync("Master");
            if (masterUsers.Count <= 1)
            {
                return BadRequest(new { success = false, message = "Não é possível alterar o cargo do único administrador Master do sistema." });
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

    [HttpPatch("{id}/toggle-status")]
    [Authorize(Policy = MasterPermissions.BlockMasterTeam)]
    public async Task<IActionResult> ToggleStatus(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return NotFound(new { success = false, message = "Usuário não encontrado." });

        var roles = await _userManager.GetRolesAsync(user);
        if (roles.Contains("Master") && user.IsActive)
        {
            var masterUsers = await _userManager.GetUsersInRoleAsync("Master");
            // Only counting active masters
            if (masterUsers.Count(u => u.IsActive) <= 1)
            {
                return BadRequest(new { success = false, message = "Não é possível bloquear o único administrador Master ativo do sistema." });
            }
        }

        user.IsActive = !user.IsActive;
        await _userManager.UpdateAsync(user);

        // Se bloqueou, invalidamos as sessões
        if (!user.IsActive)
        {
            await _userManager.UpdateSecurityStampAsync(user);
        }

        var statusName = user.IsActive ? "desbloqueado" : "bloqueado";
        return Ok(new { success = true, message = $"Usuário {statusName} com sucesso.", isActive = user.IsActive });
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = MasterPermissions.DeleteMasterTeam)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return NotFound(new { success = false, message = "Usuário não encontrado." });

        var roles = await _userManager.GetRolesAsync(user);
        if (roles.Contains("Master"))
        {
            // Evitar que o único Master seja deletado
            var masterUsers = await _userManager.GetUsersInRoleAsync("Master");
            if (masterUsers.Count <= 1)
            {
                return BadRequest(new { success = false, message = "Não é possível excluir o único administrador Master do sistema." });
            }
        }

        await _userManager.DeleteAsync(user);
        return Ok(new { success = true, message = "Usuário excluído com sucesso." });
    }
}
