using EasyPizza.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EasyPizza.Infrastructure.Data;

public class EasyPizzaDbContext : DbContext
{
    public EasyPizzaDbContext(DbContextOptions<EasyPizzaDbContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        base.OnConfiguring(optionsBuilder);
    }

    // Injeção opcional no OnConfiguring para cenários onde a connection string não foi passada nas options globais
    // Mas a melhor prática é configurar a resolução dinâmica no Program.cs



    // Catálogo
    public DbSet<ProductCategory> ProductCategories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<CategoryAddon> CategoryAddons { get; set; }
    
    // Clientes
    public DbSet<Customer> Customers { get; set; }
    public DbSet<CustomerAddress> CustomerAddresses { get; set; }
    
    // Cupons
    public DbSet<Coupon> Coupons { get; set; }
    
    // Entregadores
    public DbSet<Courier> Couriers { get; set; }
    
    // Pedidos
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<OrderItemAddon> OrderItemAddons { get; set; }
    public DbSet<OrderSession> OrderSessions { get; set; }
    public DbSet<PaymentType> PaymentTypes { get; set; }
    
    // Configurações
    public DbSet<StoreSettings> StoreSettings { get; set; }

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
            
        modelBuilder.Entity<Order>()
            .HasOne(o => o.Coupon)
            .WithMany()
            .HasForeignKey(o => o.CouponId)
            .OnDelete(DeleteBehavior.SetNull);
        
        modelBuilder.Entity<Coupon>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DiscountPercentage).HasColumnType("decimal(5,2)");
            entity.Property(e => e.DiscountFixedAmount).HasColumnType("decimal(18,2)");
        });
        
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
        });

        modelBuilder.Entity<CategoryAddon>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AdditionalPrice).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.SubTotal).HasColumnType("decimal(18,2)");
            entity.Property(e => e.DeliveryFee).HasColumnType("decimal(18,2)");
            entity.Property(e => e.DiscountAmount).HasColumnType("decimal(18,2)");
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

        modelBuilder.Entity<StoreSettings>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DeliveryFee).HasColumnType("decimal(18,2)");
            entity.Property(e => e.MinimumOrderAmount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.FreeDeliveryThreshold).HasColumnType("decimal(18,2)");
        });

        // --- Inserção de Dados Inicial (Seed) ---
        var seedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        
        var pixId = Guid.Parse("c839f99e-3151-4045-8c01-7ec829e04812");
        var moneyId = Guid.Parse("96a84920-802c-47bc-8f4e-2de9ef9f7a7d");
        var cardId = Guid.Parse("a5fb1294-f2a8-4395-8df6-cb82b95c328e");
        
        modelBuilder.Entity<PaymentType>().HasData(
            new { Id = pixId, Name = "PIX", IsOnlinePayment = true, IsActive = true, DisplayOrder = 1, CreatedAt = seedDate },
            new { Id = moneyId, Name = "Dinheiro", IsOnlinePayment = false, IsActive = true, DisplayOrder = 2, CreatedAt = seedDate },
            new { Id = cardId, Name = "Cartão (Maquininha)", IsOnlinePayment = false, IsActive = true, DisplayOrder = 3, CreatedAt = seedDate }
        );



    }
}
