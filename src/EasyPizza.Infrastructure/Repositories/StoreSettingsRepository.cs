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
            // Pré-configurações essenciais para uma nova loja iniciar operando lisinha (exceto WhatsApp que é manual)
            settings = new StoreSettings(
                isStoreOpen: true,
                deliveryFee: 5.00m,
                minimumOrderAmount: 30.00m,
                estimatedDeliveryTimeMin: 30,
                estimatedDeliveryTimeMax: 45,
                freeDeliveryThreshold: null,
                acceptingPickup: true,
                acceptingDelivery: true,
                messageOfTheDay: "🍕 Bem-vindo à nossa Pizzaria! Peças artesanais feitas com muito carinho e ingredientes selecionados.",
                activeGlobalCouponCode: null,
                whatsappBotEnabled: false,
                whatsappServerUrl: "",
                whatsappInstanceName: "",
                whatsappApiKey: "",
                whatsappSupportPhone: "",
                whatsappGreetingMessage: "Olá! Bem-vindo ao nosso atendimento automático! 🍕\n\nDigite 1 para acessar nosso Cardápio Digital\nDigite 2 para Falar com Atendente"
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
