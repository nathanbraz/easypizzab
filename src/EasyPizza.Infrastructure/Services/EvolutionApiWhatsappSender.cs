using System.Net.Http.Json;
using EasyPizza.Application.Interfaces.Repositories;
using EasyPizza.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace EasyPizza.Infrastructure.Services;

public class EvolutionApiWhatsappSender : IWhatsappSender
{
    private readonly HttpClient _httpClient;
    private readonly IStoreSettingsRepository _settingsRepository;
    private readonly ILogger<EvolutionApiWhatsappSender> _logger;

    public EvolutionApiWhatsappSender(
        HttpClient httpClient,
        IStoreSettingsRepository settingsRepository,
        ILogger<EvolutionApiWhatsappSender> logger)
    {
        _httpClient = httpClient;
        _settingsRepository = settingsRepository;
        _logger = logger;
    }

    public async Task SendTextMessageAsync(string phone, string text)
    {
        var settings = await _settingsRepository.GetSettingsAsync();

        var cleanPhone = new string(phone.Where(char.IsDigit).ToArray());

        // Se o servidor ou a API Key não estiverem configurados, loga no console de forma humanizada e limpa
        if (string.IsNullOrWhiteSpace(settings.WhatsappServerUrl) || string.IsNullOrWhiteSpace(settings.WhatsappApiKey))
        {
            _logger.LogInformation("==============================================================");
            _logger.LogInformation("[WHATSAPP MOCK SENDER] MENSAGEM GERADA PELO ROBÔ (MODO SIMULAÇÃO / TESTE):");
            _logger.LogInformation("PARA: {Phone}", cleanPhone);
            _logger.LogInformation("TEXTO:\n{Text}", text);
            _logger.LogInformation("==============================================================");
            return;
        }

        try
        {
            var serverUrl = settings.WhatsappServerUrl.TrimEnd('/');
            var instanceName = !string.IsNullOrWhiteSpace(settings.WhatsappInstanceName) ? settings.WhatsappInstanceName : "pizzariabrazil";

            string url;
            object payload;
            var request = new HttpRequestMessage(HttpMethod.Post, "");

            // Compatibilidade automática com UltraMsg (para testes na nuvem) e Evolution API (para produção/Docker)
            if (serverUrl.Contains("ultramsg.com", StringComparison.OrdinalIgnoreCase))
            {
                url = $"{serverUrl}/{instanceName}/messages/chat";
                payload = new
                {
                    token = settings.WhatsappApiKey,
                    to = cleanPhone,
                    body = text
                };
                request.RequestUri = new Uri(url);
                request.Content = JsonContent.Create(payload);
            }
            else
            {
                url = $"{serverUrl}/message/sendText/{instanceName}";
                payload = new
                {
                    number = cleanPhone,
                    text = text,
                    delay = 1200 // 1.2s de digitação simulada para humanizar o bot
                };
                request.RequestUri = new Uri(url);
                request.Headers.Add("apikey", settings.WhatsappApiKey);
                request.Content = JsonContent.Create(payload);
            }

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("[WHATSAPP SENDER ERROR] Status: {Status} | Resposta: {Error}", response.StatusCode, errorBody);
            }
            else
            {
                _logger.LogInformation("[WHATSAPP SENDER SUCESSO] Mensagem enviada para {Phone}", cleanPhone);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[WHATSAPP SENDER EXCEPTION] Erro ao tentar enviar mensagem para o Evolution API no endereço: {Url}", settings.WhatsappServerUrl);
        }
    }
}
