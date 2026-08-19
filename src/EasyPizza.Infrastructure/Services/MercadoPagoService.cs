using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using EasyPizza.Application.Interfaces.Repositories;
using EasyPizza.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace EasyPizza.Infrastructure.Services;

public class MercadoPagoService : IPaymentGatewayService
{
    private const string OrdersEndpoint = "https://api.mercadopago.com/v1/orders";

    private readonly HttpClient _httpClient;
    private readonly IStoreSettingsRepository _settingsRepository;
    private readonly ILogger<MercadoPagoService> _logger;

    public MercadoPagoService(
        HttpClient httpClient,
        IStoreSettingsRepository settingsRepository,
        ILogger<MercadoPagoService> logger)
    {
        _httpClient = httpClient;
        _settingsRepository = settingsRepository;
        _logger = logger;
    }

    public async Task<PixChargeResult> CreatePixChargeAsync(int orderId, decimal amount, string customerPhone)
    {
        var settings = await _settingsRepository.GetSettingsAsync();
        if (string.IsNullOrWhiteSpace(settings.PaymentGatewayAccessToken))
            throw new InvalidOperationException("Nenhum gateway de pagamento configurado para esta loja (Configurações > Pagamentos).");

        // O Mercado Pago exige e-mail do pagador, mas este sistema só coleta telefone (fluxo via
        // WhatsApp). Um endereço sintético, determinístico a partir do telefone, satisfaz a API
        // sem precisar de um e-mail real — nunca é usado para enviar nada, é só um identificador.
        //
        // Sempre o domínio de produção: não existe mais modo sandbox nesta aplicação — o Pix do
        // Mercado Pago nem funciona em ambiente de teste (a cobrança nunca é paga de verdade lá,
        // só dá pra checar se a chamada da API funciona), então validar o fluxo completo sempre
        // foi feito com um pagamento real de valor mínimo (cupom de desconto ~99%), nunca via
        // sandbox de fato.
        var cleanPhone = new string(customerPhone.Where(char.IsDigit).ToArray());
        var payerEmail = $"cliente-{cleanPhone}@easypizza.com.br";

        // "50.00", nunca "50,00" — a cultura pt-BR usa vírgula como separador decimal, e a API
        // do Mercado Pago só aceita ponto. Usar InvariantCulture explicitamente evita esse bug.
        var amountString = amount.ToString("F2", CultureInfo.InvariantCulture);

        var requestBody = new MercadoPagoOrderRequest(
            Type: "online",
            TotalAmount: amountString,
            ExternalReference: orderId.ToString(),
            ProcessingMode: "automatic",
            Transactions: new MercadoPagoTransactions(
                Payments: [new MercadoPagoPayment(amountString, new MercadoPagoPaymentMethod("pix", "bank_transfer"))]
            ),
            Payer: new MercadoPagoPayer(payerEmail)
        );

        using var request = new HttpRequestMessage(HttpMethod.Post, OrdersEndpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.PaymentGatewayAccessToken);
        request.Headers.Add("X-Idempotency-Key", Guid.NewGuid().ToString());
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Content = JsonContent.Create(requestBody, options: JsonOptions);

        var response = await _httpClient.SendAsync(request);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("[MERCADO PAGO ERROR] Pedido #{OrderId} | Status: {Status} | Resposta: {Body}", orderId, response.StatusCode, responseBody);
            throw new InvalidOperationException($"Falha ao gerar cobrança Pix no Mercado Pago (status {(int)response.StatusCode}).");
        }

        MercadoPagoOrderResponse? parsed;
        try
        {
            parsed = JsonSerializer.Deserialize<MercadoPagoOrderResponse>(responseBody, JsonOptions);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "[MERCADO PAGO ERROR] Pedido #{OrderId} | Resposta em formato inesperado: {Body}", orderId, responseBody);
            throw new InvalidOperationException("Resposta inesperada do Mercado Pago ao gerar cobrança Pix.");
        }

        var copyPasteCode = parsed?.Transactions?.Payments?.FirstOrDefault()?.PaymentMethod?.QrCode;
        if (parsed?.Id == null || string.IsNullOrWhiteSpace(copyPasteCode))
        {
            _logger.LogWarning("[MERCADO PAGO ERROR] Pedido #{OrderId} | Resposta sem código Pix: {Body}", orderId, responseBody);
            throw new InvalidOperationException("O Mercado Pago não retornou um código Pix válido.");
        }

        return new PixChargeResult(parsed.Id, copyPasteCode);
    }

    // Fonte única de verdade sobre o status real de uma cobrança — nunca confia em nada que
    // alguém mandou pra gente (webhook ou request manual), sempre revalida direto na API.
    public async Task<PaymentStatusResult> CheckPaymentStatusAsync(string gatewayOrderId)
    {
        var settings = await _settingsRepository.GetSettingsAsync();
        if (string.IsNullOrWhiteSpace(settings.PaymentGatewayAccessToken))
            throw new InvalidOperationException("Nenhum gateway de pagamento configurado para esta loja (Configurações > Pagamentos).");

        using var request = new HttpRequestMessage(HttpMethod.Get, $"{OrdersEndpoint}/{gatewayOrderId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.PaymentGatewayAccessToken);

        var response = await _httpClient.SendAsync(request);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("[MERCADO PAGO ERROR] Consulta de status da order {GatewayOrderId} | Status: {Status} | Resposta: {Body}", gatewayOrderId, response.StatusCode, responseBody);
            throw new InvalidOperationException($"Falha ao consultar status no Mercado Pago (status {(int)response.StatusCode}).");
        }

        var parsed = JsonSerializer.Deserialize<MercadoPagoOrderStatusResponse>(responseBody, JsonOptions);
        var payment = parsed?.Transactions?.Payments?.FirstOrDefault();

        // IMPORTANTE: "approved" (usado pela Payments API clássica) NÃO é retornado pela Orders
        // API (a que este sistema usa). Confirmado testando pagamento real de producao: o campo
        // que realmente indica "dinheiro recebido" aqui é status_detail == "accredited" — o
        // sistema ficou meses aceitando pagamentos reais sem nunca reconhecer nenhum, porque essa
        // comparação usava a string errada. status sozinho ("processed") não basta: um pagamento
        // "processed" pode ter status_detail de falha, accredited é o sinal inequívoco de sucesso.
        var isApproved = payment?.StatusDetail == "accredited";
        return new PaymentStatusResult(isApproved, parsed?.ExternalReference, payment?.Id);
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    // DTOs privados que espelham exatamente o contrato da API de Orders do Mercado Pago
    // (POST /v1/orders). Não reaproveitar essas formas fora deste arquivo — são detalhe de
    // integração, não conceitos do domínio.
    private record MercadoPagoOrderRequest(
        string Type,
        string TotalAmount,
        string ExternalReference,
        string ProcessingMode,
        MercadoPagoTransactions Transactions,
        MercadoPagoPayer Payer);

    private record MercadoPagoTransactions(List<MercadoPagoPayment> Payments);

    private record MercadoPagoPayment(string Amount, MercadoPagoPaymentMethod PaymentMethod);

    private record MercadoPagoPaymentMethod(string Id, string Type);

    private record MercadoPagoPayer(string Email);

    private record MercadoPagoOrderResponse(string? Id, MercadoPagoResponseTransactions? Transactions);

    private record MercadoPagoResponseTransactions(List<MercadoPagoResponsePayment>? Payments);

    private record MercadoPagoResponsePayment(MercadoPagoResponsePaymentMethod? PaymentMethod);

    private record MercadoPagoResponsePaymentMethod(string? QrCode);

    // Só os campos que realmente usamos da resposta de GET /v1/orders/{id} — não é o contrato
    // completo do Mercado Pago, só a fatia relevante pra confirmar pagamento.
    private record MercadoPagoOrderStatusResponse(string? ExternalReference, MercadoPagoStatusTransactions? Transactions);

    private record MercadoPagoStatusTransactions(List<MercadoPagoStatusPayment>? Payments);

    private record MercadoPagoStatusPayment(string? Id, string? Status, string? StatusDetail);
}
