using EasyPizza.Application.Interfaces.Repositories;
using EasyPizza.Domain.Entities;
using EasyPizza.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EasyPizza.Infrastructure.Repositories;

public class StoreSettingsRepository : IStoreSettingsRepository
{
    private readonly EasyPizzaDbContext _context;

    public StoreSettingsRepository(EasyPizzaDbContext context)
    {
        _context = context;
    }

    public async Task<StoreSettings> GetSettingsAsync()
    {
        var settings = await _context.StoreSettings.FirstOrDefaultAsync();
        
        if (settings == null)
        {
            // Se não existe, cria com valores padrão
            settings = new StoreSettings(
                isStoreOpen: true,
                deliveryFee: 0m,
                minimumOrderAmount: 0m,
                estimatedDeliveryTimeMin: 30,
                estimatedDeliveryTimeMax: 50,
                freeDeliveryThreshold: null,
                acceptingPickup: true,
                acceptingDelivery: true,
                messageOfTheDay: null,
                activeGlobalCouponCode: null
            );
            
            _context.StoreSettings.Add(settings);
            await _context.SaveChangesAsync();
        }
        
        return settings;
    }

    public async Task UpdateAsync(StoreSettings settings)
    {
        _context.StoreSettings.Update(settings);
        await _context.SaveChangesAsync();
    }
}
