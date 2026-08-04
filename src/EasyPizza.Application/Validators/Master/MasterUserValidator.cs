using EasyPizza.Application.DTOs.Master;
using FluentValidation;

namespace EasyPizza.Application.Validators.Master;

public class CreateMasterUserValidator : AbstractValidator<CreateMasterUserRequestDto>
{
    public CreateMasterUserValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("O nome é obrigatório.")
            .MinimumLength(3).WithMessage("O nome deve ter no mínimo 3 caracteres.")
            .MaximumLength(100).WithMessage("O nome deve ter no máximo 100 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("O e-mail é obrigatório.")
            .EmailAddress().WithMessage("O formato do e-mail é inválido.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("A senha é obrigatória.")
            .MinimumLength(6).WithMessage("A senha deve ter no mínimo 6 caracteres.");

        RuleFor(x => x.RoleName)
            .NotEmpty().WithMessage("É obrigatório selecionar um cargo para o usuário.");
    }
}
