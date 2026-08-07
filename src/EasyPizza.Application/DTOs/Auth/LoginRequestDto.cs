using System.ComponentModel.DataAnnotations;

namespace EasyPizza.Application.DTOs.Auth;

public class LoginRequestDto
{
    [Required(ErrorMessage = "O nome de usuário é obrigatório")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "A senha é obrigatória")]
    public string Password { get; set; } = string.Empty;
}
