namespace EasyPizza.Application.Interfaces.Services;

// Abstrai o gateway de pagamento configurado pela loja (StoreSettings.PaymentGatewayProvider).
// Hoje só existe implementação para o Mercado Pago, mas o consumidor (OrderService) não depende
// disso — só dessa interface, pra trocar de gateway no futuro sem tocar no fluxo de pedidos.
public interface IPaymentGatewayService
{
    Task<PixChargeResult> CreatePixChargeAsync(int orderId, decimal amount, string customerPhone);
}

// Resultado de uma cobrança Pix recém-criada no gateway.
// CopyPasteCode é o BR Code (EMV) — o mesmo texto serve tanto para o botão "copiar código"
// quanto para gerar o QR code visualmente no frontend (nenhuma imagem precisa ser armazenada).
public record PixChargeResult(string GatewayOrderId, string CopyPasteCode);
