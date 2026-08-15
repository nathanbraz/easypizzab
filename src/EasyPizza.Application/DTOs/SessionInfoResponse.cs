using EasyPizza.Domain.Entities;

namespace EasyPizza.Application.DTOs;

public class SessionInfoResponse
{
    public Guid SessionId { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhoneNumber { get; set; } = string.Empty;
    public List<CustomerAddress> Addresses { get; set; } = new();

    // Conveniência pro front não precisar procurar na lista — sempre o mesmo objeto que já está em Addresses.
    public CustomerAddress? DefaultAddress { get; set; }
    public int? LastOrderId { get; set; }
    public string? LastOrderSummary { get; set; }
}
