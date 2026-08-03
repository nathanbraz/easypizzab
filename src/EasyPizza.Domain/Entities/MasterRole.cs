using Microsoft.AspNetCore.Identity;

namespace EasyPizza.Domain.Entities;

public class MasterRole : IdentityRole<Guid>
{
    public MasterRole() : base()
    {
    }

    public MasterRole(string roleName) : base(roleName)
    {
    }
}
