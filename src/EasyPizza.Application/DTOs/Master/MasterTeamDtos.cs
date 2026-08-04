namespace EasyPizza.Application.DTOs.Master;

public record CreateMasterRoleRequestDto(string Name, List<string> Permissions);

public record UpdateMasterRoleRequestDto(string Name, List<string> Permissions);

public record CreateMasterUserRequestDto(string Name, string Email, string Password, string RoleName);
