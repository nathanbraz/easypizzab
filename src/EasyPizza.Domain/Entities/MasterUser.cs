using Microsoft.AspNetCore.Identity;

namespace EasyPizza.Domain.Entities;

public class MasterUser : IdentityUser<Guid>
{
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
