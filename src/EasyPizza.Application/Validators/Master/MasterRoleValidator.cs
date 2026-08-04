using EasyPizza.Application.DTOs.Master;
using EasyPizza.Domain.Constants;
using FluentValidation;

namespace EasyPizza.Application.Validators.Master;

public class CreateMasterRoleValidator : AbstractValidator<CreateMasterRoleRequestDto>
{
    public CreateMasterRoleValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("O nome do cargo é obrigatório.")
            .MinimumLength(3).WithMessage("O nome do cargo deve ter pelo menos 3 caracteres.")
            .MaximumLength(50).WithMessage("O nome do cargo deve ter no máximo 50 caracteres.")
            .NotEqual("Master", StringComparer.OrdinalIgnoreCase).WithMessage("O nome 'Master' é reservado pelo sistema.");

        RuleFor(x => x.Permissions)
            .NotNull().WithMessage("A lista de permissões não pode ser nula.");

        RuleForEach(x => x.Permissions)
            .Must(permission => MasterPermissions.All.Contains(permission))
            .WithMessage(x => $"A permissão informada é inválida.");
    }
}

public class UpdateMasterRoleValidator : AbstractValidator<UpdateMasterRoleRequestDto>
{
    public UpdateMasterRoleValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("O nome do cargo é obrigatório.")
            .MinimumLength(3).WithMessage("O nome do cargo deve ter pelo menos 3 caracteres.")
            .MaximumLength(50).WithMessage("O nome do cargo deve ter no máximo 50 caracteres.")
            .NotEqual("Master", StringComparer.OrdinalIgnoreCase).WithMessage("O nome 'Master' é reservado pelo sistema.");

        RuleFor(x => x.Permissions)
            .NotNull().WithMessage("A lista de permissões não pode ser nula.");

        RuleForEach(x => x.Permissions)
            .Must(permission => MasterPermissions.All.Contains(permission))
            .WithMessage(x => $"A permissão informada é inválida.");
    }
}
