using EasyPizza.Application.Interfaces.Services;
using EasyPizza.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace EasyPizza.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    // Endpoint do Cliente: Fazer um pedido
    [HttpPost("{tenantSlug}")]
    public async Task<IActionResult> CreateOrder(string tenantSlug, [FromBody] CreateOrderRequest request)
    {
        try 
        {
            var items = request.Items.Select(i => (i.ProductId, i.Quantity, i.UnitPrice)).ToList();
            var order = await _orderService.CreateOrderAsync(request.CustomerId, request.CustomerAddressId, request.Type, request.PaymentTypeId, items, request.CouponCode);
            
            return Ok(new
            {
                success = true,
                message = "Pedido realizado com sucesso!",
                data = order
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, error = ex.Message });
        }
    }

    // Endpoint do Cliente: Obter pedido por id
    [HttpGet("{tenantSlug}/{orderId:int}")]
    public async Task<IActionResult> GetOrderById(string tenantSlug, int orderId)
    {
        var order = await _orderService.GetOrderByIdAsync(orderId);
        if (order == null) return NotFound(new { success = false, message = "Pedido não encontrado." });
        return Ok(new { success = true, data = order });
    }

    // Endpoint do Cliente: Obter pedidos por cliente
    [HttpGet("{tenantSlug}/customer/{customerId:guid}")]
    public async Task<IActionResult> GetOrdersByCustomer(string tenantSlug, Guid customerId)
    {
        var orders = await _orderService.GetOrdersByCustomerAsync(customerId);
        return Ok(new { success = true, data = orders });
    }

    // Endpoint do Admin: KDS (Sistema de Tela de Cozinha)
    [HttpGet("admin/{tenantSlug}")]
    public async Task<IActionResult> GetOrders(string tenantSlug)
    {
        var orders = await _orderService.GetOrdersAsync();
        return Ok(orders);
    }

    // Endpoint do Admin: Atualizar Status do KDS
    [HttpPatch("admin/{tenantSlug}/{orderId:int}/status")]
    public async Task<IActionResult> UpdateStatus(string tenantSlug, int orderId, [FromBody] UpdateStatusRequest request)
    {
        await _orderService.UpdateOrderStatusAsync(orderId, request.Status);
        return NoContent();
    }
}

public record CreateOrderRequest(Guid CustomerId, Guid? CustomerAddressId, OrderType Type, Guid PaymentTypeId, List<OrderItemRequest> Items, string? CouponCode = null);
public record OrderItemRequest(Guid ProductId, int Quantity, decimal UnitPrice);
public record UpdateStatusRequest(OrderStatus Status);
