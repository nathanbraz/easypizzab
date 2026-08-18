using System.Globalization;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using EasyPizza.Application.Interfaces.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace EasyPizza.Api.Controllers;

// Recebe as notificações de pagamento do gateway configurado (hoje só Mercado Pago).
// Rota inclui {tenantSlug} pra que o ITenantProvider já resolva o banco certo antes de qualquer
// repositório ser usado — mesmo mecanismo já usado pelo webhook do WhatsApp.
[ApiController]
[Route("api/webhook/mercadopago")]
public class PaymentWebhookController : ControllerBase
{
    private readonly IStoreSettingsRepository _settingsRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<PaymentWebhookController> _logger;

    public PaymentWebhookController(
        IStoreSettingsRepository settingsRepository,
        IOrderRepository orderRepository,
        IHttpClientFactory httpClientFactory,
        ILogger<PaymentWebhookController> logger)
    {
        _settingsRepository = settingsRepository;
        _orderRepository = orderRepository;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    [HttpPost("{tenantSlug}")]
    public async Task<IActionResult> ReceiveNotification(string tenantSlug, [FromBody] MercadoPagoWebhookPayload? payload)
    {
        // 200 só pro que é genuinamente "não me interessa" — evento de um tipo que a gente nem
        // trata. Aqui não tem nada pra corrigir depois, então não faz sentido o Mercado Pago
        // retentar (mesma postura do webhook do WhatsApp, que também descarta ruído assim).
        if (payload?.Type != "order" || string.IsNullOrWhiteSpace(payload.Data?.Id))
        {
            _logger.LogInformation("[WEBHOOK MERCADO PAGO] Notificação ignorada (tipo: {Type}) para {Tenant}", payload?.Type, tenantSlug);
            return Ok();
        }

        var settings = await _settingsRepository.GetSettingsAsync();
        if (string.IsNullOrWhiteSpace(settings.PaymentGatewayAccessToken))
        {
            _logger.LogWarning("[WEBHOOK MERCADO PAGO] Notificação recebida para {Tenant}, mas não há gateway configurado.", tenantSlug);
            // Não-2xx de propósito: o Mercado Pago retenta automaticamente com backoff em
            // resposta != 2xx (confirmado na documentação deles). Se alguém configurar o gateway
            // a tempo de uma dessas retentativas, o pagamento se resolve sozinho, sem intervenção
            // manual — descartar com 200 aqui perderia essa rede de segurança.
            return StatusCode(StatusCodes.Status503ServiceUnavailable);
        }

        // Validação de assinatura: só processa a notificação se PaymentGatewayWebhookSecret
        // estiver configurado. Sem o segredo configurado, ninguém consegue provar que a chamada
        // veio de verdade do Mercado Pago — melhor não processar do que confiar nela.
        if (string.IsNullOrWhiteSpace(settings.PaymentGatewayWebhookSecret))
        {
            _logger.LogWarning("[WEBHOOK MERCADO PAGO] Segredo do webhook não configurado para {Tenant}.", tenantSlug);
            return StatusCode(StatusCodes.Status503ServiceUnavailable);
        }

        if (!IsSignatureValid(Request, payload.Data.Id, settings.PaymentGatewayWebhookSecret))
        {
            _logger.LogWarning("[WEBHOOK MERCADO PAGO] Assinatura inválida na notificação para {Tenant} (order {GatewayOrderId}).", tenantSlug, payload.Data.Id);
            // 401, não 200: uma assinatura inválida pode ser um segredo desatualizado na nossa
            // configuração (aconteceu de verdade nesta sessão) — nesse caso, corrigir o segredo
            // e deixar o Mercado Pago retentar resolve sozinho. Devolver 200 aqui faria o Mercado
            // Pago achar que já entregou com sucesso e nunca mais reenviar essa notificação.
            return Unauthorized();
        }

        try
        {
            await ProcessOrderNotificationAsync(payload.Data.Id, settings.PaymentGatewayAccessToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[WEBHOOK MERCADO PAGO] Falha ao processar notificação da order {GatewayOrderId} para {Tenant}", payload.Data.Id, tenantSlug);
            // Idem: erro transitório (rede, banco, etc.) merece nova tentativa do lado do
            // Mercado Pago, não um 200 que encerra a entrega pro lado deles.
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        return Ok();
    }

    private async Task ProcessOrderNotificationAsync(string gatewayOrderId, string accessToken)
    {
        var client = _httpClientFactory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.mercadopago.com/v1/orders/{gatewayOrderId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await client.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("[WEBHOOK MERCADO PAGO] Falha ao buscar detalhes da order {GatewayOrderId}: {Status} | {Body}", gatewayOrderId, response.StatusCode, body);
            return;
        }

        var orderDetails = JsonSerializer.Deserialize<MercadoPagoOrderDetails>(body, JsonOptions);
        var externalReference = orderDetails?.ExternalReference;
        var payment = orderDetails?.Transactions?.Payments?.FirstOrDefault();

        if (string.IsNullOrWhiteSpace(externalReference) || !int.TryParse(externalReference, out var orderId))
        {
            _logger.LogWarning("[WEBHOOK MERCADO PAGO] Order {GatewayOrderId} sem external_reference válido: {Body}", gatewayOrderId, body);
            return;
        }

        // "approved" é o status universal do Mercado Pago para pagamento confirmado em todas as
        // APIs deles (Payments API clássica, e o valor nesse campo aninhado da Orders API). Os
        // outros valores (pending/in_process/rejected etc.) significam que ainda não é pra
        // liberar o pedido.
        if (payment?.Status != "approved")
        {
            _logger.LogInformation("[WEBHOOK MERCADO PAGO] Pedido #{OrderId}: status ainda não aprovado ({Status})", orderId, payment?.Status);
            return;
        }

        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
        {
            _logger.LogWarning("[WEBHOOK MERCADO PAGO] Pedido #{OrderId} (referenciado pela order {GatewayOrderId}) não encontrado", orderId, gatewayOrderId);
            return;
        }

        // Idempotência: o Mercado Pago pode reenviar a mesma notificação mais de uma vez.
        // Sem essa checagem, MarkAsPaid rodaria de novo e re-moveria o pedido pra "Preparando".
        if (order.IsPaid)
            return;

        order.MarkAsPaid(payment.Id);
        await _orderRepository.SaveChangesAsync();

        _logger.LogInformation("[WEBHOOK MERCADO PAGO] Pedido #{OrderId} marcado como pago (pagamento {PaymentId})", orderId, payment.Id);
    }

    // Confirma que a notificação realmente veio do Mercado Pago, validando a assinatura HMAC-SHA256
    // do header X-Signature contra o segredo do webhook configurado pela loja.
    //
    // NOTA: o algoritmo abaixo segue o padrão publicamente documentado pelo Mercado Pago (manifest
    // "id:{data.id};request-id:{x-request-id};ts:{ts};", HMAC-SHA256 com o segredo do webhook),
    // reconstruído a partir de múltiplas fontes já que não consegui abrir a página oficial completa
    // com o algoritmo passo a passo durante o desenvolvimento. Validar com uma notificação real do
    // painel do Mercado Pago antes de confiar 100% nisso em produção.
    private static bool IsSignatureValid(HttpRequest request, string dataId, string webhookSecret)
    {
        if (!request.Headers.TryGetValue("x-signature", out var signatureHeader) ||
            !request.Headers.TryGetValue("x-request-id", out var requestIdHeader))
            return false;

        string? ts = null;
        string? v1 = null;
        foreach (var part in signatureHeader.ToString().Split(','))
        {
            var kv = part.Split('=', 2);
            if (kv.Length != 2) continue;
            var key = kv[0].Trim();
            var value = kv[1].Trim();
            if (key == "ts") ts = value;
            else if (key == "v1") v1 = value;
        }

        if (string.IsNullOrEmpty(ts) || string.IsNullOrEmpty(v1))
            return false;

        var manifest = $"id:{dataId.ToLowerInvariant()};request-id:{requestIdHeader};ts:{ts};";
        var computedHash = ComputeHmacSha256Hex(manifest, webhookSecret);

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(computedHash),
            Encoding.UTF8.GetBytes(v1));
    }

    private static string ComputeHmacSha256Hex(string message, string secret)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };
}

public record MercadoPagoWebhookPayload(string? Type, string? Action, MercadoPagoWebhookData? Data);

public record MercadoPagoWebhookData(string? Id);

// Só os campos que realmente usamos da resposta de GET /v1/orders/{id} — não é o contrato
// completo do Mercado Pago, só a fatia relevante pra confirmar pagamento.
public record MercadoPagoOrderDetails(string? ExternalReference, MercadoPagoOrderTransactions? Transactions);

public record MercadoPagoOrderTransactions(List<MercadoPagoOrderPayment>? Payments);

public record MercadoPagoOrderPayment(string? Id, string? Status);
