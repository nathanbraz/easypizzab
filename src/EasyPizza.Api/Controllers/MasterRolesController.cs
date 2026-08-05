using EasyPizza.Application.DTOs.Master;
using EasyPizza.Domain.Constants;
using EasyPizza.Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EasyPizza.Api.Controllers;

[Authorize(Policy = "RequireMaster")]
[ApiController]
[Route("api/master/roles")]
public class MasterRolesController : ControllerBase
{
    private readonly RoleManager<MasterRole> _roleManager;
    private readonly UserManager<MasterUser> _userManager;
    private readonly IValidator<CreateMasterRoleRequestDto> _createValidator;
    private readonly IValidator<UpdateMasterRoleRequestDto> _updateValidator;

    public MasterRolesController(
        RoleManager<MasterRole> roleManager,
        UserManager<MasterUser> userManager,
        IValidator<CreateMasterRoleRequestDto> createValidator,
        IValidator<UpdateMasterRoleRequestDto> updateValidator)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet("permissions")]
    public IActionResult GetAvailablePermissions()
    {
        // Retorna a lista de permissões traduzida para o Frontend renderizar checkboxes
        var permissions = MasterPermissions.All.Select(p => new {
            Id = p,
            Name = TranslatePermission(p)
        });
        return Ok(new { success = true, data = permissions });
    }

    [HttpGet]
    [Authorize(Policy = MasterPermissions.ViewMasterRoles)]
    public async Task<IActionResult> GetAll()
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

    [HttpPost]
    [Authorize(Policy = MasterPermissions.CreateMasterRoles)]
    public async Task<IActionResult> Create([FromBody] CreateMasterRoleRequestDto request)
    {
        var validationResult = await _createValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { success = false, errors = validationResult.Errors.Select(e => e.ErrorMessage) });
        }

        if (await _roleManager.RoleExistsAsync(request.Name))
        {
            return BadRequest(new { success = false, message = "Já existe um cargo com esse nome." });
        }

        var role = new MasterRole(request.Name);
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

    [HttpPut("{id}")]
    [Authorize(Policy = MasterPermissions.EditMasterRoles)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMasterRoleRequestDto request)
    {
        var validationResult = await _updateValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { success = false, errors = validationResult.Errors.Select(e => e.ErrorMessage) });
        }

        var role = await _roleManager.FindByIdAsync(id.ToString());
        if (role == null) return NotFound(new { success = false, message = "Cargo não encontrado." });

        if (role.Name == "Master") return BadRequest(new { success = false, message = "Não é permitido alterar o cargo nativo Master." });

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

    [HttpDelete("{id}")]
    [Authorize(Policy = MasterPermissions.DeleteMasterRoles)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var role = await _roleManager.FindByIdAsync(id.ToString());
        if (role == null) return NotFound(new { success = false, message = "Cargo não encontrado." });

        if (role.Name == "Master") return BadRequest(new { success = false, message = "Não é possível excluir o cargo nativo Master." });

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
            MasterPermissions.ViewTenants => "Visualizar Lojistas",
            MasterPermissions.CreateTenants => "Criar Lojistas",
            MasterPermissions.EditTenants => "Editar Lojistas",
            MasterPermissions.BlockTenants => "Bloquear/Desbloquear Lojistas",
            MasterPermissions.ViewMasterTeam => "Visualizar Equipe",
            MasterPermissions.CreateMasterTeam => "Criar Equipe",
            MasterPermissions.EditMasterTeam => "Editar Equipe",
            MasterPermissions.BlockMasterTeam => "Inativar/Bloquear Equipe",
            MasterPermissions.DeleteMasterTeam => "Excluir Equipe",
            MasterPermissions.ViewMasterRoles => "Visualizar Cargos",
            MasterPermissions.CreateMasterRoles => "Criar Cargos",
            MasterPermissions.EditMasterRoles => "Editar Cargos",
            MasterPermissions.DeleteMasterRoles => "Excluir Cargos",
            MasterPermissions.ViewBilling => "Visualizar Faturamento",
            MasterPermissions.ManageBilling => "Gerenciar Faturamento",
            _ => permissionCode
        };
    }
}
