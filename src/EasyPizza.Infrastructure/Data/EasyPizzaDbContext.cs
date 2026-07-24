using EasyPizza.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EasyPizza.Infrastructure.Data;

public class EasyPizzaDbContext : DbContext
{
    public EasyPizzaDbContext(DbContextOptions<EasyPizzaDbContext> options) : base(options)
    {
    }

    // Injeção opcional no OnConfiguring para cenários onde a connection string não foi passada nas options globais
    // Mas a melhor prática é configurar a resolução dinâmica no Program.cs



    // Catalog
    public DbSet<ProductCategory> ProductCategories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductAddon> ProductAddons { get; set; }
    
    // Customers
    public DbSet<Customer> Customers { get; set; }
    public DbSet<CustomerAddress> CustomerAddresses { get; set; }
    
    // Orders
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<OrderItemAddon> OrderItemAddons { get; set; }
    public DbSet<OrderSession> OrderSessions { get; set; }
    public DbSet<PaymentType> PaymentTypes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // --- Configurações das Entidades ---
        
        modelBuilder.Entity<Customer>()
            .HasIndex(c => c.PhoneNumber)
            .IsUnique();

        modelBuilder.Entity<Customer>()
            .HasMany(c => c.Addresses)
            .WithOne(ca => ca.Customer)
            .HasForeignKey(ca => ca.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Customer>()
            .HasMany(c => c.Sessions)
            .WithOne(os => os.Customer)
            .HasForeignKey(os => os.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Order>()
            .HasOne(o => o.Customer)
            .WithMany()
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Order>()
            .HasOne(o => o.Address)
            .WithMany()
            .HasForeignKey(o => o.CustomerAddressId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Order>()
            .HasOne(o => o.PaymentType)
            .WithMany()
            .HasForeignKey(o => o.PaymentTypeId)
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<OrderSession>(entity =>
        {
            entity.HasKey(e => e.Id);
        });
        
        modelBuilder.Entity<ProductCategory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasMany(e => e.Products).WithOne(e => e.Category).HasForeignKey(e => e.CategoryId);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
            entity.HasMany(e => e.Addons).WithOne(e => e.Product).HasForeignKey(e => e.ProductId);
        });

        modelBuilder.Entity<ProductAddon>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AdditionalPrice).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SubTotal).HasColumnType("decimal(18,2)");
            entity.Property(e => e.DeliveryFee).HasColumnType("decimal(18,2)");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
            entity.HasMany(e => e.Items).WithOne(e => e.Order).HasForeignKey(e => e.OrderId);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
            entity.HasMany(e => e.Addons).WithOne(e => e.OrderItem).HasForeignKey(e => e.OrderItemId);
        });

        modelBuilder.Entity<OrderItemAddon>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
        });

        // --- Data Seeding ---
        var seedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        
        var pixId = Guid.Parse("c839f99e-3151-4045-8c01-7ec829e04812");
        var moneyId = Guid.Parse("96a84920-802c-47bc-8f4e-2de9ef9f7a7d");
        var cardId = Guid.Parse("a5fb1294-f2a8-4395-8df6-cb82b95c328e");
        
        modelBuilder.Entity<PaymentType>().HasData(
            new { Id = pixId, Name = "PIX", IsOnlinePayment = true, IsActive = true, DisplayOrder = 1, CreatedAt = seedDate },
            new { Id = moneyId, Name = "Dinheiro", IsOnlinePayment = false, IsActive = true, DisplayOrder = 2, CreatedAt = seedDate },
            new { Id = cardId, Name = "Cartão (Maquininha)", IsOnlinePayment = false, IsActive = true, DisplayOrder = 3, CreatedAt = seedDate }
        );

        var pizzaCatId = Guid.Parse("d866a152-4467-4d7a-8f4b-bfb6df7d6b38");
        var drinkCatId = Guid.Parse("41b711ea-eb8d-4ab0-b5cc-44b2f676451e");

        modelBuilder.Entity<ProductCategory>().HasData(
            new { Id = pizzaCatId, Name = "Pizzas Tradicionais", DisplayOrder = 1, CreatedAt = seedDate },
            new { Id = drinkCatId, Name = "Bebidas", DisplayOrder = 2, CreatedAt = seedDate }
        );

        modelBuilder.Entity<Product>().HasData(
            new { Id = Guid.Parse("8ebde348-18e3-4c07-b358-fc24d1eb4df4"), CategoryId = pizzaCatId, Name = "Calabresa", Description = "Muçarela, calabresa e cebola", Price = 49.90m, IsAvailable = true, CreatedAt = seedDate },
            new { Id = Guid.Parse("a0f7c1d3-3b12-4c28-98e3-f61b0c034298"), CategoryId = pizzaCatId, Name = "Marguerita", Description = "Muçarela, tomate e manjericão fresco", Price = 45.00m, IsAvailable = true, CreatedAt = seedDate },
            new { Id = Guid.Parse("18f3a382-3d84-46b2-a4f6-8c4d28d0b8c4"), CategoryId = drinkCatId, Name = "Coca-Cola 2L", Description = "Refrigerante 2 Litros", Price = 14.00m, IsAvailable = true, CreatedAt = seedDate }
        );

    }
}
