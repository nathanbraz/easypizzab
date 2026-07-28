using EasyPizza.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace EasyPizza.Api.Controllers;

[ApiController]
[Route("api/webhook/whatsapp")]
public class WhatsappWebhookController : ControllerBase
{
    private readonly IWhatsappBotService _whatsappBotService;
    private readonly ILogger<WhatsappWebhookController> _logger;

    public WhatsappWebhookController(
        IWhatsappBotService whatsappBotService,
        ILogger<WhatsappWebhookController> logger)
    {
        _whatsappBotService = whatsappBotService;
        _logger = logger;
    }

    [HttpPost("{instanceName}")]
    public async Task<IActionResult> ReceiveWebhook(string instanceName, [FromBody] EvolutionWebhookPayload? payload)
    {
        if (payload == null)
        {
            _logger.LogWarning("[WEBHOOK WHATSAPP] Payload recebido é nulo para a instância {Instance}", instanceName);
            return Ok(); // Sempre retornar 200 OK para evitar retentativas infinitas do motor de WhatsApp
        }

        // Tenta extrair telefone e texto tanto do formato simplificado (Swagger) quanto do formato Evolution API
        string? phone = payload.Phone ?? payload.Data?.Key?.RemoteJid?.Split('@')[0];
        string? text = payload.Text ?? payload.Data?.Message?.Conversation ?? payload.Data?.Message?.ExtendedTextMessage?.Text;
        string? senderName = payload.Name ?? payload.PushName ?? payload.SenderName ?? payload.Data?.PushName ?? payload.Data?.NotifyName ?? payload.Sender;

        if (string.IsNullOrWhiteSpace(phone) || string.IsNullOrWhiteSpace(text))
        {
            _logger.LogInformation("[WEBHOOK WHATSAPP] Mensagem sem texto legível ou telefone ignorada. Instância: {Instance}", instanceName);
            return Ok();
        }

        // Ignora mensagens enviadas pelo próprio robô ou dono da conta (fromMe == true no Evolution API) para evitar loop infinito
        if (payload.Data?.Key?.FromMe == true)
        {
            return Ok();
        }

        _logger.LogInformation("[WEBHOOK RECEBIDO] Instância: {Instance} | De: {Phone} | Nome: {Name} | Texto: {Text}", instanceName, phone, senderName ?? "N/A", text);

        await _whatsappBotService.ProcessIncomingMessageAsync(instanceName, phone, text, senderName);

        return Ok(new { success = true, message = "Mensagem processada pelo robô com sucesso" });
    }
}

// DTOs flexíveis para suportar tanto testes rápidos no Swagger quanto o padrão Evolution API / Z-API
public class EvolutionWebhookPayload
{
    // Campos simplificados para teste no Swagger/Postman
    public string? Phone { get; set; }
    public string? Text { get; set; }
    public string? Name { get; set; }
    public string? PushName { get; set; }
    public string? SenderName { get; set; }
    public string? Sender { get; set; }

    // Campos reais do payload Evolution API / Z-API
    public EvolutionData? Data { get; set; }
}

public class EvolutionData
{
    public EvolutionKey? Key { get; set; }
    public EvolutionMessage? Message { get; set; }
    public string? PushName { get; set; }
    public string? NotifyName { get; set; }
}

public class EvolutionKey
{
    public string? RemoteJid { get; set; }
    public bool? FromMe { get; set; }
}

public class EvolutionMessage
{
    public string? Conversation { get; set; }
    public EvolutionExtendedText? ExtendedTextMessage { get; set; }
}

public class EvolutionExtendedText
{
    public string? Text { get; set; }
}
