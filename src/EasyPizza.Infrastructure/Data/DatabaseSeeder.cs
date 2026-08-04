using System.Security.Claims;
using EasyPizza.Domain.Constants;
using EasyPizza.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EasyPizza.Infrastructure.Data;

public static class DatabaseSeeder
{
    public static async Task SeedMasterUserAsync(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        using var scope = serviceProvider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<MasterUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<MasterRole>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseSeeder");

        var masterEmail = configuration["MasterDefault:Email"];
        var masterPassword = configuration["MasterDefault:Password"];
        var masterName = configuration["MasterDefault:Name"];

        if (string.IsNullOrEmpty(masterEmail) || string.IsNullOrEmpty(masterPassword))
        {
            logger.LogWarning("MasterDefault credentials not found in appsettings.json. Skipping MasterUser seed.");
            return;
        }

        // Criar Role se não existir
        var masterRole = await roleManager.FindByNameAsync("Master");
        if (masterRole == null)
        {
            masterRole = new MasterRole("Master");
            await roleManager.CreateAsync(masterRole);
        }

        // Semear as permissões (claims) para o Master
        var existingClaims = await roleManager.GetClaimsAsync(masterRole);
        foreach (var permission in MasterPermissions.All)
        {
            if (!existingClaims.Any(c => c.Type == "Permission" && c.Value == permission))
            {
                await roleManager.AddClaimAsync(masterRole, new Claim("Permission", permission));
            }
        }

        // Criar MasterUser se não existir
        var existingUser = await userManager.FindByEmailAsync(masterEmail);
        if (existingUser == null)
        {
            var masterUser = new MasterUser
            {
                UserName = masterEmail,
                Email = masterEmail,
                Name = masterName ?? "Admin",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(masterUser, masterPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(masterUser, "Master");
                logger.LogInformation("MasterUser {Email} created successfully.", masterEmail);
            }
            else
            {
                logger.LogError("Error creating MasterUser: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
        else
        {
            logger.LogInformation("MasterUser {Email} already exists. Skipping seed.", masterEmail);
        }
    }
}
