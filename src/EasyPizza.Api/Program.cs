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

// Add services to the container.
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

// DI for Master DB
builder.Services.AddDbContext<MasterDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("MasterConnection")));

// DI for Tenant Resolution
builder.Services.AddScoped<ITenantProvider, HttpTenantProvider>();

// DI for Tenant DB (Dynamic Connection String)
builder.Services.AddDbContext<EasyPizzaDbContext>((serviceProvider, options) =>
{
    var tenantProvider = serviceProvider.GetRequiredService<ITenantProvider>();
    var connectionString = tenantProvider.GetConnectionString();

    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException("Erro de SaaS: Tenant (Pizzaria) não identificado ou não cadastrado no Banco Mestre.");
    }

    options.UseNpgsql(connectionString);
});

// DI for Repositories and Services
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

// Configure the HTTP request pipeline.
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
app.MapControllers();

app.Run();
