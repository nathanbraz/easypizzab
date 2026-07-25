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

    // Customer Endpoint: Place an order
    [HttpPost("{tenantSlug}")]
    public async Task<IActionResult> CreateOrder(string tenantSlug, [FromBody] CreateOrderRequest request)
    {
        try 
        {
            var items = request.Items.Select(i => (i.ProductId, i.Quantity, i.UnitPrice)).ToList();
            var order = await _orderService.CreateOrderAsync(request.CustomerId, request.CustomerAddressId, request.Type, request.PaymentTypeId, items);
            
            return CreatedAtAction(nameof(GetOrders), new { tenantSlug = tenantSlug }, order);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // Admin Endpoint: KDS
    [HttpGet("admin/{tenantSlug}")]
    public async Task<IActionResult> GetOrders(string tenantSlug)
    {
        var orders = await _orderService.GetOrdersAsync();
        return Ok(orders);
    }

    // Admin Endpoint: Update KDS Status
    [HttpPatch("admin/{tenantSlug}/{orderId:guid}/status")]
    public async Task<IActionResult> UpdateStatus(string tenantSlug, Guid orderId, [FromBody] UpdateStatusRequest request)
    {
        await _orderService.UpdateOrderStatusAsync(orderId, request.Status);
        return NoContent();
    }
}

public record CreateOrderRequest(Guid CustomerId, Guid? CustomerAddressId, OrderType Type, Guid PaymentTypeId, List<OrderItemRequest> Items);
public record OrderItemRequest(Guid ProductId, int Quantity, decimal UnitPrice);
public record UpdateStatusRequest(OrderStatus Status);
