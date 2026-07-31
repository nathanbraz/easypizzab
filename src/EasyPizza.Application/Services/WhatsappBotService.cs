using EasyPizza.Application.DTOs;
using EasyPizza.Application.Interfaces;
using EasyPizza.Application.Interfaces.Repositories;
using EasyPizza.Application.Interfaces.Services;

namespace EasyPizza.Application.Services;

public class WhatsappBotService : IWhatsappBotService
{
    private readonly IStoreSettingsRepository _settingsRepository;
    private readonly ISessionService _sessionService;
    private readonly IWhatsappSender _whatsappSender;
    private readonly ITenantProvider _tenantProvider;

    public WhatsappBotService(
        IStoreSettingsRepository settingsRepository,
        ISessionService sessionService,
        IWhatsappSender whatsappSender,
        ITenantProvider tenantProvider)
    {
        _settingsRepository = settingsRepository;
        _sessionService = sessionService;
        _whatsappSender = whatsappSender;
        _tenantProvider = tenantProvider;
    }

    public async Task<string> GenerateOrderLinkAsync(string phone, string? name = null)
    {
        var sessionResponse = await _sessionService.GenerateMagicLinkSessionAsync(new GenerateSessionRequest
        {
            PhoneNumber = phone,
            Name = name
        });

        var tenant = _tenantProvider.GetTenant();
        var tenantSlug = tenant?.Slug ?? "pizzariabrazil";

        // Retorna o link mágico formatado com subdomínio lvh.me em desenvolvimento local
        var baseUrl = $"http://{tenantSlug}.lvh.me:3333";
        return $"{baseUrl}/?t={sessionResponse.SessionId}";
    }

    public async Task ProcessIncomingMessageAsync(string instanceName, string senderPhone, string messageText, string? senderName = null)
    {
        var settings = await _settingsRepository.GetSettingsAsync();

        // Se o robô estiver desativado nas configurações da loja, ignora em silêncio
        if (!settings.WhatsappBotEnabled)
            return;

        var cleanText = messageText?.Trim().ToLower() ?? "";
        var cleanPhone = senderPhone?.Trim() ?? "";

        if (string.IsNullOrEmpty(cleanPhone))
            return;

        // Máquina de estados simples do Menu Interativo
        if (cleanText == "1" || cleanText == "um" || cleanText == "cardapio" || cleanText == "cardápio" || cleanText == "pedido" || cleanText == "fazer pedido")
        {
            var orderLink = await GenerateOrderLinkAsync(cleanPhone, senderName);
            var responseText = $"Que ótimo! 🍕 Clique no link abaixo para acessar nosso Cardápio Digital com seu token de segurança exclusivo e fazer seu pedido rapidamente:\n\n👉 {orderLink}\n\n*Nota: Este link é autenticado e exclusivo para o seu WhatsApp!*";
            await _whatsappSender.SendTextMessageAsync(cleanPhone, responseText);
        }
        else if (cleanText == "2" || cleanText == "dois" || cleanText == "atendente" || cleanText == "falar com atendente" || cleanText == "suporte")
        {
            var supportPhone = !string.IsNullOrEmpty(settings.WhatsappSupportPhone) ? settings.WhatsappSupportPhone : "da nossa loja";
            var responseText = $"Entendido! 👨‍🍳 Estamos transferindo o seu atendimento.\n\nPor favor, aguarde um instante ou chame diretamente nosso suporte humano no número: *{supportPhone}*. Em breve alguém falará com você!";
            await _whatsappSender.SendTextMessageAsync(cleanPhone, responseText);
        }
        else
        {
            // Resposta de Boas-Vindas & Cardápio Interativo
            var greeting = !string.IsNullOrEmpty(settings.WhatsappGreetingMessage)
                ? settings.WhatsappGreetingMessage
                : "Olá! Bem-vindo ao atendimento automático da Pizzaria Brazil! 🍕\n\nDigite *1* para acessar nosso Cardápio Digital\nDigite *2* para Falar com Atendente";

            await _whatsappSender.SendTextMessageAsync(cleanPhone, greeting);
        }
    }
}
