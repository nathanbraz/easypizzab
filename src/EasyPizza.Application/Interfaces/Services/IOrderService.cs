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
    // Pizza Meio a Meio (Sabores): ids dos Produtos-sabor extras escolhidos, além deste próprio
    // produto. Fica separado de Addons porque cada sabor é um Produto inteiro, não uma opção de
    // catálogo solta — mas a validação (elegibilidade, min/máx) usa o mesmo grupo compartilhado
    // da categoria (IsFlavorGroup) que Tamanho/Borda/Adicionais já usam. Preço sempre recalculado
    // no backend (OrderService), nunca confiado daqui.
    List<Guid>? FlavorProductIds = null,
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
