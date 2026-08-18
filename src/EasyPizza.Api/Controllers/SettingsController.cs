using EasyPizza.Application.Interfaces.Repositories;
using EasyPizza.Domain.Entities;
using EasyPizza.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EasyPizza.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SettingsController : ControllerBase
{
    private readonly IStoreSettingsRepository _settingsRepository;
    private readonly EasyPizzaDbContext _context;

    public SettingsController(IStoreSettingsRepository settingsRepository, EasyPizzaDbContext context)
    {
        _settingsRepository = settingsRepository;
        _context = context;
    }

    // Público de propósito: o cardápio e o checkout do cliente final (sem JWT de staff) precisam
    // saber se a loja está aberta, taxa de entrega, pedido mínimo e as formas de pagamento disponíveis.
    // IMPORTANTE: essa rota não tem autenticação — por isso devolve só um DTO com o que é
    // realmente público, nunca a entidade inteira (que carrega credenciais como WhatsappApiKey
    // e PaymentGatewayAccessToken).
    [HttpGet]
    public async Task<IActionResult> GetSettings()
    {
        var settings = await _settingsRepository.GetSettingsAsync();
        var paymentTypes = await _context.PaymentTypes.OrderBy(p => p.DisplayOrder).ToListAsync();

        return Ok(new
        {
            StoreSettings = PublicStoreSettingsDto.From(settings),
            PaymentTypes = paymentTypes
        });
    }

    // Autenticado: alimenta a tela de Configurações do admin. Mostra tudo que o lojista precisa
    // ver e editar, exceto os valores de credenciais já salvas — pra essas, só um indicador
    // booleano (HasWhatsappApiKey/HasPaymentGatewayAccessToken). O valor em si nunca volta pro
    // navegador depois de salvo, só é possível sobrescrevê-lo (ver UpdateSettings).
    [Authorize(Policy = "RequireTenant")]
    [HttpGet("admin")]
    public async Task<IActionResult> GetSettingsForAdmin()
    {
        var settings = await _settingsRepository.GetSettingsAsync();
        var paymentTypes = await _context.PaymentTypes.OrderBy(p => p.DisplayOrder).ToListAsync();

        return Ok(new
        {
            StoreSettings = AdminStoreSettingsDto.From(settings),
            PaymentTypes = paymentTypes
        });
    }

    [Authorize(Policy = "RequireTenant")]
    [HttpPut]
    public async Task<IActionResult> UpdateSettings([FromBody] UpdateSettingsRequest request)
    {
        var settings = await _settingsRepository.GetSettingsAsync();

        // Campos de credencial: o formulário do admin nunca reenvia o segredo já salvo (ele nem
        // sabe qual é), só manda um valor novo quando o lojista realmente quer trocá-lo. Em
        // branco/nulo aqui significa "manter o que já está salvo", não "apagar".
        // (WhatsappApiKey não entra aqui de propósito — só o Master define, via TenantsController.)

        // Só existe integração com o Mercado Pago hoje, então o provider é sempre esse quando um
        // token novo é salvo. Quando outro gateway existir, isso vira um valor vindo do request.
        var paymentGatewayAccessToken = string.IsNullOrWhiteSpace(request.PaymentGatewayAccessToken) ? settings.PaymentGatewayAccessToken : request.PaymentGatewayAccessToken;
        var paymentGatewayProvider = string.IsNullOrWhiteSpace(request.PaymentGatewayAccessToken) ? settings.PaymentGatewayProvider : "MercadoPago";
        var paymentGatewayWebhookSecret = string.IsNullOrWhiteSpace(request.PaymentGatewayWebhookSecret) ? settings.PaymentGatewayWebhookSecret : request.PaymentGatewayWebhookSecret;

        settings.Update(
            request.IsStoreOpen,
            request.DeliveryFee,
            request.MinimumOrderAmount,
            request.EstimatedDeliveryTimeMin,
            request.EstimatedDeliveryTimeMax,
            request.FreeDeliveryThreshold,
            request.AcceptingPickup,
            request.AcceptingDelivery,
            request.MessageOfTheDay,
            request.ActiveGlobalCouponCode,
            request.WhatsappBotEnabled,
            request.WhatsappSupportPhone,
            request.WhatsappGreetingMessage,
            request.LogoUrl,
            request.BannerUrl,
            paymentGatewayProvider,
            paymentGatewayAccessToken,
            paymentGatewayWebhookSecret,
            request.PaymentGatewaySandboxMode
        );

        await _settingsRepository.UpdateAsync(settings);

        return Ok(AdminStoreSettingsDto.From(settings));
    }
    
    [Authorize(Policy = "RequireTenant")]
    [HttpPut("payment-types/{id}/toggle")]
    public async Task<IActionResult> TogglePaymentType(Guid id, [FromBody] TogglePaymentRequest request)
    {
        var paymentType = await _context.PaymentTypes.FindAsync(id);
        if (paymentType == null) return NotFound(new { error = "Meio de pagamento não encontrado" });

        paymentType.ToggleActive(request.IsActive);

        await _context.SaveChangesAsync();
        return Ok(paymentType);
    }
}

public record UpdateSettingsRequest(
    bool IsStoreOpen,
    decimal DeliveryFee,
    decimal MinimumOrderAmount,
    int EstimatedDeliveryTimeMin,
    int EstimatedDeliveryTimeMax,
    decimal? FreeDeliveryThreshold,
    bool AcceptingPickup,
    bool AcceptingDelivery,
    string? MessageOfTheDay,
    string? ActiveGlobalCouponCode,
    bool WhatsappBotEnabled,
    string? WhatsappSupportPhone,
    string? WhatsappGreetingMessage,
    string? LogoUrl = null,
    string? BannerUrl = null,
    string? PaymentGatewayAccessToken = null,
    string? PaymentGatewayWebhookSecret = null,
    bool PaymentGatewaySandboxMode = true
);

public record TogglePaymentRequest(bool IsActive);

// Formato público de StoreSettings: só os campos que o cardápio/checkout do cliente final
// realmente usa. Nenhuma credencial (WhatsApp, gateway de pagamento) passa por aqui de propósito.
public record PublicStoreSettingsDto(
    bool IsStoreOpen,
    decimal DeliveryFee,
    decimal MinimumOrderAmount,
    int EstimatedDeliveryTimeMin,
    int EstimatedDeliveryTimeMax,
    decimal? FreeDeliveryThreshold,
    bool AcceptingPickup,
    bool AcceptingDelivery,
    string? MessageOfTheDay,
    string? ActiveGlobalCouponCode,
    string? LogoUrl,
    string? BannerUrl)
{
    public static PublicStoreSettingsDto From(StoreSettings settings) => new(
        settings.IsStoreOpen,
        settings.DeliveryFee,
        settings.MinimumOrderAmount,
        settings.EstimatedDeliveryTimeMin,
        settings.EstimatedDeliveryTimeMax,
        settings.FreeDeliveryThreshold,
        settings.AcceptingPickup,
        settings.AcceptingDelivery,
        settings.MessageOfTheDay,
        settings.ActiveGlobalCouponCode,
        settings.LogoUrl,
        settings.BannerUrl);
}

// Formato de StoreSettings pro admin autenticado: tudo que ele pode configurar, mas credenciais
// (WhatsappApiKey, PaymentGatewayAccessToken) viram só um booleano "já configurado" — o valor em
// texto puro nunca é devolvido pelo backend depois de salvo.
public record AdminStoreSettingsDto(
    bool IsStoreOpen,
    decimal DeliveryFee,
    decimal MinimumOrderAmount,
    int EstimatedDeliveryTimeMin,
    int EstimatedDeliveryTimeMax,
    decimal? FreeDeliveryThreshold,
    bool AcceptingPickup,
    bool AcceptingDelivery,
    string? MessageOfTheDay,
    string? ActiveGlobalCouponCode,
    string? LogoUrl,
    string? BannerUrl,
    bool WhatsappBotEnabled,
    bool HasWhatsappApiKey,
    string? WhatsappSupportPhone,
    string? WhatsappGreetingMessage,
    string? PaymentGatewayProvider,
    bool HasPaymentGatewayAccessToken,
    bool HasPaymentGatewayWebhookSecret,
    bool PaymentGatewaySandboxMode)
{
    public static AdminStoreSettingsDto From(StoreSettings settings) => new(
        settings.IsStoreOpen,
        settings.DeliveryFee,
        settings.MinimumOrderAmount,
        settings.EstimatedDeliveryTimeMin,
        settings.EstimatedDeliveryTimeMax,
        settings.FreeDeliveryThreshold,
        settings.AcceptingPickup,
        settings.AcceptingDelivery,
        settings.MessageOfTheDay,
        settings.ActiveGlobalCouponCode,
        settings.LogoUrl,
        settings.BannerUrl,
        settings.WhatsappBotEnabled,
        !string.IsNullOrWhiteSpace(settings.WhatsappApiKey),
        settings.WhatsappSupportPhone,
        settings.WhatsappGreetingMessage,
        settings.PaymentGatewayProvider,
        !string.IsNullOrWhiteSpace(settings.PaymentGatewayAccessToken),
        !string.IsNullOrWhiteSpace(settings.PaymentGatewayWebhookSecret),
        settings.PaymentGatewaySandboxMode);
}
