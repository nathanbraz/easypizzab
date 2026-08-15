using EasyPizza.Application.Interfaces.Repositories;
using EasyPizza.Application.Interfaces.Services;
using EasyPizza.Domain.Entities;
using Microsoft.AspNetCore.Authorization;

namespace EasyPizza.Api.Authorization;

// Autoriza requisições do cliente final (sessão de magic link), separado do JWT de staff.
// O token vem no header X-Customer-Session e é validado contra a tabela OrderSessions do tenant atual.
public class CustomerSessionAuthorizationHandler : AuthorizationHandler<CustomerSessionRequirement>
{
    public const string HeaderName = "X-Customer-Session";

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IServiceProvider _serviceProvider;
    private readonly ICurrentCustomerAccessor _currentCustomer;

    public CustomerSessionAuthorizationHandler(
        IHttpContextAccessor httpContextAccessor,
        IServiceProvider serviceProvider,
        ICurrentCustomerAccessor currentCustomer)
    {
        _httpContextAccessor = httpContextAccessor;
        _serviceProvider = serviceProvider;
        _currentCustomer = currentCustomer;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, CustomerSessionRequirement requirement)
    {
        var headerValue = _httpContextAccessor.HttpContext?.Request.Headers[HeaderName].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(headerValue) || !Guid.TryParse(headerValue, out var token))
            return;

        // O ASP.NET Core instancia TODOS os IAuthorizationHandler registrados em toda verificação de
        // autorização, inclusive em rotas de Master (sem tenant resolvido, ex: admin.lvh.me). Por isso
        // IRepository<OrderSession> (que depende do EasyPizzaDbContext, o banco DO TENANT) só pode ser
        // resolvido aqui dentro, depois de confirmar que há tenant — nunca via injeção no construtor,
        // senão a simples existência deste handler quebra qualquer requisição de Master/staff.
        var tenantProvider = _serviceProvider.GetRequiredService<ITenantProvider>();
        if (tenantProvider.GetTenant() == null)
            return;

        var sessionRepository = _serviceProvider.GetRequiredService<IRepository<OrderSession>>();
        var session = await sessionRepository.GetByIdAsync(token);
        if (session == null || !session.IsValid())
            return;

        _currentCustomer.CustomerId = session.CustomerId;
        _currentCustomer.SessionId = session.Id;

        context.Succeed(requirement);
    }
}
