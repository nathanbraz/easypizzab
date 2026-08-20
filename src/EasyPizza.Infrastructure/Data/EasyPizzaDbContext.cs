using EasyPizza.Domain.Constants;
using EasyPizza.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace EasyPizza.Infrastructure.Data;

public class EasyPizzaDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public EasyPizzaDbContext(DbContextOptions<EasyPizzaDbContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        base.OnConfiguring(optionsBuilder);
    }

    // A lógica de Model Creating foi unificada abaixo

    // Injeção opcional no OnConfiguring para cenários onde a connection string não foi passada nas options globais
    // Mas a melhor prática é configurar a resolução dinâmica no Program.cs



    // Catálogo
    public DbSet<ProductCategory> ProductCategories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductOptionGroup> ProductOptionGroups { get; set; }
    public DbSet<ProductOptionItem> ProductOptionItems { get; set; }
    public DbSet<CategoryOptionGroup> CategoryOptionGroups { get; set; }
    public DbSet<CategoryOptionItem> CategoryOptionItems { get; set; }
    public DbSet<ProductCategoryOptionPrice> ProductCategoryOptionPrices { get; set; }

    
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

        // Renomeando as tabelas do Identity para remover o prefixo AspNet
        modelBuilder.Entity<ApplicationUser>().ToTable("Users");
        modelBuilder.Entity<ApplicationRole>().ToTable("Roles");
        modelBuilder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
        modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
        modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
        modelBuilder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
        modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");

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
            entity.HasMany(e => e.OptionGroups).WithOne(e => e.Category).HasForeignKey(e => e.CategoryId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
            entity.HasMany(e => e.OptionGroups).WithOne(e => e.Product).HasForeignKey(e => e.ProductId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.CategoryOptionPrices).WithOne(e => e.Product).HasForeignKey(e => e.ProductId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProductOptionGroup>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasMany(e => e.Options).WithOne(e => e.Group).HasForeignKey(e => e.GroupId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProductOptionItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AdditionalPrice).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<CategoryOptionGroup>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasMany(e => e.Items).WithOne(e => e.Group).HasForeignKey(e => e.GroupId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CategoryOptionItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UniformPrice).HasColumnType("decimal(18,2)");
            entity.HasOne(e => e.Product)
                .WithMany()
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ProductCategoryOptionPrice>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AdditionalPrice).HasColumnType("decimal(18,2)");
            entity.HasIndex(e => new { e.ProductId, e.CategoryOptionItemId }).IsUnique();
            entity.HasOne(e => e.CategoryOptionItem)
                .WithMany()
                .HasForeignKey(e => e.CategoryOptionItemId)
                .OnDelete(DeleteBehavior.Cascade);
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
            
            // SetNull: se o produto for deletado, o item do pedido perde a referência, mas não é deletado (protege o histórico)
            entity.HasOne(e => e.Product).WithMany().HasForeignKey(e => e.ProductId).OnDelete(DeleteBehavior.SetNull);
            
            entity.HasMany(e => e.Addons).WithOne(e => e.OrderItem).HasForeignKey(e => e.OrderItemId);
        });

        modelBuilder.Entity<OrderItemAddon>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Quantity).HasDefaultValue(1);
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

        // --- Seed do Cargo "Owner" e suas Permissões ---
        var ownerRoleId = Guid.Parse("866de267-9cb3-4f41-8b6d-b814038798b5");
        
        modelBuilder.Entity<ApplicationRole>().HasData(
            new ApplicationRole 
            { 
                Id = ownerRoleId, 
                Name = "Administrador", 
                NormalizedName = "ADMINISTRADOR" 
            }
        );

        var roleClaims = new List<IdentityRoleClaim<Guid>>();
        int claimId = 1;

        foreach (var permission in Permissions.All)
        {
            roleClaims.Add(new IdentityRoleClaim<Guid>
            {
                Id = claimId++,
                RoleId = ownerRoleId,
                ClaimType = "Permission",
                ClaimValue = permission
            });
        }

        modelBuilder.Entity<IdentityRoleClaim<Guid>>().HasData(roleClaims);

    }
}
