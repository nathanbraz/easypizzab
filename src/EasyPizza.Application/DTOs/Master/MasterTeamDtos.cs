namespace EasyPizza.Application.DTOs.Master;

public record CreateMasterRoleRequestDto(string Name, List<string> Permissions);

public record UpdateMasterRoleRequestDto(string Name, List<string> Permissions);

public record CreateMasterUserRequestDto(string Name, string UserName, string Password, string RoleName);

public record UpdateMasterUserRequestDto(string Name, string RoleName);
