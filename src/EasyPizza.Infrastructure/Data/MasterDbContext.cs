using EasyPizza.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EasyPizza.Infrastructure.Data;

public class MasterDbContext : DbContext
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
    }
}
