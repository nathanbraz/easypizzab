using Microsoft.AspNetCore.Identity;

namespace EasyPizza.Domain.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public string Name { get; set; } = string.Empty;
}
