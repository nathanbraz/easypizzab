namespace EasyPizza.Application.Interfaces.Services;

// Guarda o CustomerId/SessionId já validados pelo RequireCustomerSession na requisição atual.
// Nunca deve ser preenchido a partir de dados enviados pelo cliente (route/body) — só pelo authorization handler.
public interface ICurrentCustomerAccessor
{
    Guid? CustomerId { get; set; }
    Guid? SessionId { get; set; }
}
