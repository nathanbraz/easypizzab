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
}

/// <summary>DTO de item de pedido com opções e observação.</summary>
public record OrderItemInput(
    Guid ProductId,
    int Quantity,
    decimal UnitPrice,
    string? Notes = null,
    List<OrderItemAddonInput>? Addons = null);

/// <summary>DTO de adicional/opção selecionada por item de pedido.</summary>
public record OrderItemAddonInput(
    Guid? ProductOptionItemId,
    string AddonName,
    decimal Price,
    int Quantity = 1);
