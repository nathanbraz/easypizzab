using EasyPizza.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace EasyPizza.Infrastructure.Data;

public class MasterDbContext : IdentityDbContext<MasterUser, MasterRole, Guid>
{
    public MasterDbContext(DbContextOptions<MasterDbContext> options) : base(options)
    {
    }

    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<GlobalSaaSSettings> GlobalSettings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.Property(e => e.Slug).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ConnectionString).IsRequired();
        });

        modelBuilder.Entity<GlobalSaaSSettings>(entity =>
        {
            entity.HasKey(e => e.Id);
        });

        // Renomeando as tabelas do Identity do MasterDB para manter limpo (MasterUsers, MasterRoles, etc)
        modelBuilder.Entity<MasterUser>().ToTable("MasterUsers");
        modelBuilder.Entity<MasterRole>().ToTable("MasterRoles");
        modelBuilder.Entity<IdentityUserRole<Guid>>().ToTable("MasterUserRoles");
        modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable("MasterUserClaims");
        modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable("MasterUserLogins");
        modelBuilder.Entity<IdentityRoleClaim<Guid>>().ToTable("MasterRoleClaims");
        modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable("MasterUserTokens");
    }
}
