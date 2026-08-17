using EasyPizza.Api.Authorization;
using EasyPizza.Api.Services;
using EasyPizza.Application.Interfaces;
using EasyPizza.Application.Interfaces.Repositories;
using EasyPizza.Application.Interfaces.Services;
using EasyPizza.Application.Services;
using EasyPizza.Domain.Entities;
using EasyPizza.Infrastructure.Data;
using EasyPizza.Infrastructure.Repositories;
using EasyPizza.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using EasyPizza.Domain.Constants;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FluentValidation;
using EasyPizza.Application.Validators.Master;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddValidatorsFromAssemblyContaining<CreateMasterRoleValidator>();

// Adiciona serviços ao contêiner.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });
builder.Services.AddOpenApi();
builder.Services.AddHttpContextAccessor();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Injeção de Dependência para o Banco de Dados Master
builder.Services.AddDbContext<MasterDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("MasterConnection")));

// Configuração do Identity para o Banco Master (Dono do SaaS)
builder.Services.AddIdentityCore<MasterUser>()
    .AddRoles<MasterRole>()
    .AddEntityFrameworkStores<MasterDbContext>()
    .AddDefaultTokenProviders();

// Injeção de Dependência para Resolução do Tenant
builder.Services.AddScoped<ITenantProvider, HttpTenantProvider>();

// Injeção de Dependência para o Banco de Dados do Tenant (String de Conexão Dinâmica)
builder.Services.AddDbContext<EasyPizzaDbContext>((serviceProvider, options) =>
{
    var tenantProvider = serviceProvider.GetRequiredService<ITenantProvider>();
    var connectionString = tenantProvider.GetConnectionString();

    if (!string.IsNullOrEmpty(connectionString))
    {
        options.UseNpgsql(connectionString);
    }
});

// Configuração do Identity para o Banco da Pizzaria (Tenants)
builder.Services.AddIdentityCore<ApplicationUser>()
    .AddRoles<ApplicationRole>()
    .AddEntityFrameworkStores<EasyPizzaDbContext>()
    .AddDefaultTokenProviders();

// Configuração do JWT Bearer
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(jwtKey)) throw new Exception("JWT Key is missing in appsettings.json");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Em prod mudar para true
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtKey)),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            var userId = context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var scope = context.Principal?.FindFirst("Scope")?.Value;
            var tokenStamp = context.Principal?.FindFirst("AspNet.Identity.SecurityStamp")?.Value;

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(scope) || string.IsNullOrEmpty(tokenStamp))
            {
                context.Fail("Invalid Token Claims");
                return;
            }

            if (scope == "Master")
            {
                var masterUserManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<MasterUser>>();
                var user = await masterUserManager.FindByIdAsync(userId);
                if (user == null || user.SecurityStamp != tokenStamp)
                {
                    context.Fail("Token Revoked");
                }
            }
            else if (scope == "Tenant")
            {
                var tenantUserManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
                var user = await tenantUserManager.FindByIdAsync(userId);
                if (user == null || user.SecurityStamp != tokenStamp)
                {
                    context.Fail("Token Revoked");
                }
            }
        }
    };
});

builder.Services.AddAuthorization(options =>
{
    // Escopos base de isolamento
    options.AddPolicy("RequireMaster", policy => policy.RequireClaim("Scope", "Master"));
    options.AddPolicy("RequireTenant", policy => policy.RequireClaim("Scope", "Tenant"));

    // Políticas Granulares PBAC baseadas em Claims (Tenant)
    foreach (var permission in EasyPizza.Domain.Constants.Permissions.All)
    {
        options.AddPolicy(permission, policy => 
        {
            policy.RequireClaim("Scope", "Tenant"); // Apenas lojistas têm permissões granulares
            policy.RequireClaim("Permission", permission);
        });
    }

    // Políticas Granulares PBAC baseadas em Claims (Master)
    foreach (var permission in EasyPizza.Domain.Constants.MasterPermissions.All)
    {
        options.AddPolicy(permission, policy =>
        {
            policy.RequireClaim("Scope", "Master");
            policy.RequireClaim("Permission", permission);
        });
    }

    // Sessão do cliente final (magic link via WhatsApp) — não é JWT, ver CustomerSessionAuthorizationHandler
    options.AddPolicy("RequireCustomerSession", policy => policy.Requirements.Add(new CustomerSessionRequirement()));
});

builder.Services.AddScoped<IAuthorizationHandler, CustomerSessionAuthorizationHandler>();

// Injeção de Dependência para Repositórios e Serviços
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<ICatalogRepository, CatalogRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<ICatalogService, CatalogService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<ICurrentCustomerAccessor, CurrentCustomerAccessor>();
builder.Services.AddScoped<ICouponRepository, CouponRepository>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<ICourierRepository, CourierRepository>();
builder.Services.AddScoped<IStoreSettingsRepository, StoreSettingsRepository>();
builder.Services.AddScoped<IPaymentTypeRepository, PaymentTypeRepository>();
builder.Services.AddHttpClient<IWhatsappSender, EvolutionApiWhatsappSender>();
builder.Services.AddScoped<IWhatsappBotService, WhatsappBotService>();
builder.Services.AddHttpClient<IPaymentGatewayService, MercadoPagoService>();

var app = builder.Build();

// Rodar o Migrador Automático e o Seeder do Master (MasterUser)
using (var scope = app.Services.CreateScope())
{
    // 1. Atualiza as tabelas de todos os bancos (Master e Pizzarias)
    await DatabaseMigrator.MigrateDatabasesAsync(scope.ServiceProvider);

    // 2. Injeta o seu usuário Master
    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    await DatabaseSeeder.SeedMasterUserAsync(scope.ServiceProvider, config);
}

// Configura o pipeline de requisições HTTP.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowAll");

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

// Middleware de bloqueio de Tenant
app.Use(async (context, next) =>
{
    // Ignora rotas nativas que não precisam de banco de dados do lojista
    if (context.Request.Method == "OPTIONS" ||
        context.Request.Path.StartsWithSegments("/openapi") || 
        context.Request.Path.StartsWithSegments("/swagger") ||
        context.Request.Path.StartsWithSegments("/api/master") ||
        context.Request.Path.StartsWithSegments("/api/auth/login") ||
        context.Request.Path.StartsWithSegments("/api/uploads"))
    {
        await next(context);
        return;
    }

    var tenantProvider = context.RequestServices.GetRequiredService<ITenantProvider>();
    var tenant = tenantProvider.GetTenant();
    
    if (tenant == null || string.IsNullOrEmpty(tenant.ConnectionString))
    {
        context.Response.StatusCode = 400; // Bad Request
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync("{\"success\": false, \"message\": \"Erro de SaaS: Empresa não identificada ou não cadastrada no Banco Mestre.\"}");
        return; 
    }

    if (!tenant.IsActive)
    {
        context.Response.StatusCode = 403; // Forbidden
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync("{\"success\": false, \"message\": \"TenantSuspended\"}");
        return; 
    }

    await next(context);
});

app.MapControllers();


app.Run();
