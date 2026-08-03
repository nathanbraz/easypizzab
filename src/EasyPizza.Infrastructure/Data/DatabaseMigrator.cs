using EasyPizza.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EasyPizza.Infrastructure.Data;

public static class DatabaseMigrator
{
    public static async Task MigrateDatabasesAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseMigrator");

        try
        {
            logger.LogInformation("Iniciando migração do Banco de Dados Master...");
            var masterDb = scope.ServiceProvider.GetRequiredService<MasterDbContext>();
            await masterDb.Database.MigrateAsync();
            logger.LogInformation("Banco Master atualizado com sucesso.");

            logger.LogInformation("Buscando Tenants para migrar os bancos isolados...");
            var tenants = await masterDb.Tenants.ToListAsync();

            foreach (var tenant in tenants)
            {
                logger.LogInformation("Migrando banco do Tenant: {TenantName} ({Slug})", tenant.Name, tenant.Slug);
                
                // Cria as options com a string de conexão específica do tenant
                var optionsBuilder = new DbContextOptionsBuilder<EasyPizzaDbContext>();
                optionsBuilder.UseNpgsql(tenant.ConnectionString);

                using var tenantDb = new EasyPizzaDbContext(optionsBuilder.Options);
                await tenantDb.Database.MigrateAsync();
                
                logger.LogInformation("Banco do Tenant {Slug} atualizado.", tenant.Slug);
            }
            
            logger.LogInformation("Todas as migrações foram concluídas com sucesso.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ocorreu um erro crítico durante a migração dos bancos de dados.");
            throw; // Derruba a API se o banco não puder ser atualizado (Segurança)
        }
    }
}
