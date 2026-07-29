using EasyPizza.Api.Services;
using EasyPizza.Application.Interfaces;
using EasyPizza.Application.Interfaces.Repositories;
using EasyPizza.Application.Interfaces.Services;
using EasyPizza.Application.Services;
using EasyPizza.Infrastructure.Data;
using EasyPizza.Infrastructure.Repositories;
using EasyPizza.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

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

// Injeção de Dependência para Repositórios e Serviços
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<ICatalogRepository, CatalogRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<ICatalogService, CatalogService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<ICouponRepository, CouponRepository>();
builder.Services.AddScoped<ICourierRepository, CourierRepository>();
builder.Services.AddScoped<IStoreSettingsRepository, StoreSettingsRepository>();
builder.Services.AddScoped<IPaymentTypeRepository, PaymentTypeRepository>();
builder.Services.AddHttpClient<IWhatsappSender, EvolutionApiWhatsappSender>();
builder.Services.AddScoped<IWhatsappBotService, WhatsappBotService>();

var app = builder.Build();

// Configura o pipeline de requisição HTTP.
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
app.UseAuthorization();

// Middleware de bloqueio de Tenant
app.Use(async (context, next) =>
{
    // Ignora rotas nativas que não precisam de banco de dados do lojista
    if (context.Request.Method == "OPTIONS" ||
        context.Request.Path.StartsWithSegments("/openapi") || 
        context.Request.Path.StartsWithSegments("/swagger") ||
        context.Request.Path.StartsWithSegments("/api/superadmin") ||
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
