using EasyPizza.Domain.Entities;

namespace EasyPizza.Application.Interfaces.Services;

public interface IOrderService
{
    Task<Order> CreateOrderAsync(
        Guid customerId,
        Guid? customerAddressId,
        OrderType type,
        Guid paymentTypeId,
        List<OrderItemInput> items,
        string? couponCode = null,
        decimal? changeFor = null);

    Task<IEnumerable<Order>> GetOrdersAsync();
    Task<Order?> GetOrderByIdAsync(int orderId);
    Task<IEnumerable<Order>> GetOrdersByCustomerAsync(Guid customerId);
    Task UpdateOrderStatusAsync(int orderId, OrderStatus status);
    Task CancelOrderAsync(int orderId, string reason);
}

/// <summary>DTO de item de pedido com opções e observação.</summary>
public record OrderItemInput(
    Guid ProductId,
    int Quantity,
    decimal UnitPrice,
    string? Notes = null,
    List<OrderItemAddonInput>? Addons = null,
    // Pizza Meio a Meio: id do produto escolhido pra 2ª metade. Fica separado de Addons de
    // propósito — não é uma opção de catálogo (Tamanho/Borda/Adicionais), é outro produto inteiro
    // combinado com este. O preço da diferença é sempre recalculado no backend (OrderService),
    // nunca confiado daqui.
    Guid? SecondHalfProductId = null,
    // Marca que este item veio do carrossel "Aproveite e leve também" do checkout — se o produto
    // tiver Product.CrossSellDiscountPrice configurado, o backend usa esse preço em vez do preço
    // normal (ver OrderService). Só um sinalizador; o valor do desconto em si nunca vem do cliente.
    bool IsCrossSell = false);

/// <summary>DTO de adicional/opção selecionada por item de pedido.</summary>
public record OrderItemAddonInput(
    Guid? ProductOptionItemId,
    string AddonName,
    decimal Price,
    int Quantity = 1);
