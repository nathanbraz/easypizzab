using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EasyPizza.Infrastructure.Data;

public class EasyPizzaDbContextFactory : IDesignTimeDbContextFactory<EasyPizzaDbContext>
{
    public EasyPizzaDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<EasyPizzaDbContext>();
        
        // This is a dummy connection string used ONLY for design-time tooling (EF Core Migrations).
        // The actual connection string is resolved dynamically at runtime by ITenantProvider in Program.cs.
        optionsBuilder.UseNpgsql("Host=localhost;Database=dummy_migration;Username=postgres;Password=postgres");

        return new EasyPizzaDbContext(optionsBuilder.Options);
    }
}
