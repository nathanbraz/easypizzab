using EasyPizza.Application.DTOs;
using EasyPizza.Application.Interfaces;
using EasyPizza.Application.Interfaces.Repositories;
using EasyPizza.Application.Interfaces.Services;
using EasyPizza.Domain.Entities;

namespace EasyPizza.Application.Services;

public class SessionService : ISessionService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IRepository<OrderSession> _sessionRepository;
    private readonly ITenantProvider _tenantProvider;
    private readonly IOrderRepository _orderRepository;

    public SessionService(
        ICustomerRepository customerRepository,
        IRepository<OrderSession> sessionRepository,
        ITenantProvider tenantProvider,
        IOrderRepository orderRepository)
    {
        _customerRepository = customerRepository;
        _sessionRepository = sessionRepository;
        _tenantProvider = tenantProvider;
        _orderRepository = orderRepository;
    }

    public async Task<GenerateSessionResponse> GenerateMagicLinkSessionAsync(GenerateSessionRequest request)
    {
        var tenant = _tenantProvider.GetTenant();
        if (tenant == null)
            throw new Exception("Tenant not resolved. Cannot generate session.");

        // Buscar cliente ou criar um novo
        var customer = await _customerRepository.GetByPhoneNumberAsync(request.PhoneNumber);
        
        if (customer == null)
        {
            var customerName = !string.IsNullOrWhiteSpace(request.Name) ? request.Name.Trim() : $"Cliente ({request.PhoneNumber})";
            customer = new Customer(request.PhoneNumber, customerName);
            await _customerRepository.AddAsync(customer);
            await _customerRepository.SaveChangesAsync();
        }
        else if (!string.IsNullOrWhiteSpace(request.Name) && customer.Name != request.Name.Trim())
        {
            customer.UpdateName(request.Name.Trim());
            await _customerRepository.UpdateAsync(customer);
            await _customerRepository.SaveChangesAsync();
        }

        // Criar a sessão
        var session = new OrderSession(customer.Id);
        await _sessionRepository.AddAsync(session);
        await _sessionRepository.SaveChangesAsync();

        // O link mágico gerado com o Slug do Tenant
        // BaseUrl será configurado no frontend. O backend só devolve o Path
        var magicLink = $"/{tenant.Slug}?t={session.Id}";

        return new GenerateSessionResponse
        {
            SessionId = session.Id,
            MagicLink = magicLink
        };
    }

    public async Task<SessionInfoResponse?> GetSessionInfoAsync(Guid token)
    {
        var session = await _sessionRepository.GetByIdAsync(token);
        if (session == null || !session.IsValid())
            return null;

        var customer = await _customerRepository.GetByIdAsync(session.CustomerId);
        if (customer == null)
            return null;

        var lastOrder = await _orderRepository.GetLastCustomerOrderAsync(customer.Id);
        
        string? summary = null;
        if (lastOrder != null && lastOrder.Items.Any())
        {
            var firstItem = lastOrder.Items.First();
            summary = $"{lastOrder.Items.Count} item(s) - {firstItem.Product?.Name} {(lastOrder.Items.Count > 1 ? "..." : "")}";
        }

        var defaultAddress = customer.Addresses.OrderByDescending(a => a.CreatedAt).FirstOrDefault();

        return new SessionInfoResponse
        {
            SessionId = session.Id,
            CustomerId = customer.Id,
            CustomerName = customer.Name ?? "Visitante",
            CustomerPhoneNumber = customer.PhoneNumber,
            DefaultAddress = defaultAddress,
            LastOrderId = lastOrder?.Id,
            LastOrderSummary = summary
        };
    }
}
