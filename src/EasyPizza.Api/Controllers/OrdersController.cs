using EasyPizza.Application.Interfaces;
using EasyPizza.Application.Interfaces.Services;
using EasyPizza.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace EasyPizza.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ISessionService _sessionService;
    private readonly ICurrentCustomerAccessor _currentCustomer;

    public OrdersController(IOrderService orderService, ISessionService sessionService, ICurrentCustomerAccessor currentCustomer)
    {
        _orderService = orderService;
        _sessionService = sessionService;
        _currentCustomer = currentCustomer;
    }

    // Endpoint do Cliente: Fazer um pedido.
    // Exige sessão de cliente válida (magic link) — o CustomerId nunca vem do payload, só da sessão validada.
    [Authorize(Policy = "RequireCustomerSession")]
    [HttpPost("{tenantSlug}")]
    public async Task<IActionResult> CreateOrder(string tenantSlug, [FromBody] CreateOrderRequest request)
    {
        try
        {
            var customerId = _currentCustomer.CustomerId!.Value;
            var items = request.Items.Select(i => new OrderItemInput(
                i.ProductId,
                i.Quantity,
                i.UnitPrice,
                i.Notes,
                i.Addons?.Select(a => new OrderItemAddonInput(a.ProductOptionItemId, a.AddonName, a.Price, a.Quantity)).ToList()
            )).ToList();

            var order = await _orderService.CreateOrderAsync(customerId, request.CustomerAddressId, request.Type, request.PaymentTypeId, items, request.CouponCode, request.ChangeFor);

            // Pedido concluído: a sessão do magic link não serve mais. Pra pedir de novo, volta ao WhatsApp.
            await _sessionService.MarkSessionAsUsedAsync(_currentCustomer.SessionId!.Value);

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
    [Authorize(Policy = "RequireTenant")]
    [HttpGet("admin/{tenantSlug}")]
    public async Task<IActionResult> GetOrders(string tenantSlug)
    {
        var orders = await _orderService.GetOrdersAsync();
        return Ok(orders);
    }

    // Endpoint do Admin: Atualizar Status do KDS
    [Authorize(Policy = "RequireTenant")]
    [HttpPatch("admin/{tenantSlug}/{orderId:int}/status")]
    public async Task<IActionResult> UpdateStatus(string tenantSlug, int orderId, [FromBody] UpdateStatusRequest request)
    {
        await _orderService.UpdateOrderStatusAsync(orderId, request.Status);
        return NoContent();
    }
}

public record CreateOrderRequest(Guid? CustomerAddressId, OrderType Type, Guid PaymentTypeId, List<OrderItemRequest> Items, string? CouponCode = null, decimal? ChangeFor = null);
public record OrderItemRequest(Guid ProductId, int Quantity, decimal UnitPrice, string? Notes = null, List<OrderItemAddonRequest>? Addons = null);
public record OrderItemAddonRequest(Guid? ProductOptionItemId, string AddonName, decimal Price, int Quantity = 1);
public record UpdateStatusRequest(OrderStatus Status);
