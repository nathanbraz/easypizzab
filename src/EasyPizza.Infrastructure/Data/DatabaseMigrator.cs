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

        // Banco Master: falha aqui derruba a API de propósito — sem ele nenhuma loja é resolvida de
        // qualquer forma (toda requisição consulta a tabela Tenants nele pra achar a connection string).
        logger.LogInformation("Iniciando migração do Banco de Dados Master...");
        var masterDb = scope.ServiceProvider.GetRequiredService<MasterDbContext>();
        try
        {
            await masterDb.Database.MigrateAsync();
            logger.LogInformation("Banco Master atualizado com sucesso.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ocorreu um erro crítico durante a migração do Banco Master.");
            throw; // Derruba a API — sem o Master nenhuma loja funciona de qualquer forma
        }

        // Bancos dos tenants: cada loja tem seu próprio banco isolado. Um erro numa loja específica
        // (banco fora do ar, connection string desatualizada, schema alterado manualmente etc.) não
        // deve derrubar a API inteira nem impedir as outras lojas de funcionar — só fica registrado
        // no log, e pode ser corrigido depois via POST /master/Tenants/{slug}/migrate.
        logger.LogInformation("Buscando Tenants para migrar os bancos isolados...");
        var tenants = await masterDb.Tenants.ToListAsync();

        foreach (var tenant in tenants)
        {
            try
            {
                logger.LogInformation("Migrando banco do Tenant: {TenantName} ({Slug})", tenant.Name, tenant.Slug);

                var optionsBuilder = new DbContextOptionsBuilder<EasyPizzaDbContext>();
                optionsBuilder.UseNpgsql(tenant.ConnectionString);

                using var tenantDb = new EasyPizzaDbContext(optionsBuilder.Options);
                await tenantDb.Database.MigrateAsync();

                logger.LogInformation("Banco do Tenant {Slug} atualizado.", tenant.Slug);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Falha ao migrar o banco do Tenant {Slug}. Essa loja pode ficar indisponível até o problema ser corrigido manualmente (ex: via POST /master/Tenants/{Slug}/migrate). As demais lojas seguem normalmente.", tenant.Slug, tenant.Slug);
            }
        }

        logger.LogInformation("Migração de bancos concluída (ver logs acima para eventuais falhas por tenant).");
    }
}
