using EasyPizza.Application.DTOs;
using EasyPizza.Application.Interfaces;
using EasyPizza.Application.Interfaces.Repositories;
using EasyPizza.Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;

namespace EasyPizza.Application.Services;

public class WhatsappBotService : IWhatsappBotService
{
    private readonly IStoreSettingsRepository _settingsRepository;
    private readonly ISessionService _sessionService;
    private readonly IWhatsappSender _whatsappSender;
    private readonly ITenantProvider _tenantProvider;
    private readonly IConfiguration _configuration;

    public WhatsappBotService(
        IStoreSettingsRepository settingsRepository,
        ISessionService sessionService,
        IWhatsappSender whatsappSender,
        ITenantProvider tenantProvider,
        IConfiguration configuration)
    {
        _settingsRepository = settingsRepository;
        _sessionService = sessionService;
        _whatsappSender = whatsappSender;
        _tenantProvider = tenantProvider;
        _configuration = configuration;
    }

    public async Task<string?> GenerateOrderLinkAsync(string phone, string? name = null)
    {
        // Sem tenant resolvido não há como saber pra qual loja o link deve apontar —
        // melhor não gerar um link errado do que arriscar apontar pra outra loja (fallback antigo era fixo).
        var tenant = _tenantProvider.GetTenant();
        if (tenant == null)
            return null;

        var sessionResponse = await _sessionService.GenerateMagicLinkSessionAsync(new GenerateSessionRequest
        {
            PhoneNumber = phone,
            Name = name
        });

        // "{slug}" é substituído pelo slug do tenant atual. Em dev aponta pro Vite local (lvh.me);
        // em produção, configurar via variável de ambiente Frontend__BaseUrlTemplate.
        var baseUrlTemplate = _configuration["Frontend:BaseUrlTemplate"] ?? "http://{slug}.lvh.me:3333";
        var baseUrl = baseUrlTemplate.Replace("{slug}", tenant.Slug);

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
            if (orderLink == null)
                return; // Loja não identificada a partir do webhook — nada seguro a fazer aqui

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
