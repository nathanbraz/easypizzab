using EasyPizza.Application.Interfaces.Repositories;
using EasyPizza.Domain.Entities;
using EasyPizza.Infrastructure.Data;
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

    [HttpGet]
    public async Task<IActionResult> GetSettings()
    {
        var settings = await _settingsRepository.GetSettingsAsync();
        var paymentTypes = await _context.PaymentTypes.OrderBy(p => p.DisplayOrder).ToListAsync();

        return Ok(new
        {
            StoreSettings = settings,
            PaymentTypes = paymentTypes
        });
    }

    [HttpPut]
    public async Task<IActionResult> UpdateSettings([FromBody] UpdateSettingsRequest request)
    {
        var settings = await _settingsRepository.GetSettingsAsync();

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
            request.WhatsappServerUrl,
            request.WhatsappInstanceName,
            request.WhatsappApiKey,
            request.WhatsappSupportPhone,
            request.WhatsappGreetingMessage
        );

        await _settingsRepository.UpdateAsync(settings);

        return Ok(settings);
    }
    
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
    string? WhatsappServerUrl,
    string? WhatsappInstanceName,
    string? WhatsappApiKey,
    string? WhatsappSupportPhone,
    string? WhatsappGreetingMessage
);

public record TogglePaymentRequest(bool IsActive);
