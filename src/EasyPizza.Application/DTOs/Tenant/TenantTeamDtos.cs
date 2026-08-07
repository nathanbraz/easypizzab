using System.ComponentModel.DataAnnotations;

namespace EasyPizza.Application.DTOs.Tenant;

public record CreateTenantRoleRequestDto(
    [Required] string Name, 
    [Required] List<string> Permissions);

public record UpdateTenantRoleRequestDto(
    [Required] string Name, 
    [Required] List<string> Permissions);

public record CreateTenantUserRequestDto(
    [Required] string Name, 
    [Required] string UserName, 
    [Required] string Password, 
    [Required] string RoleName);

public record UpdateTenantUserRequestDto(
    [Required] string Name, 
    [Required] string RoleName);
