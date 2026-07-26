using EasyPizza.Domain.Entities;

namespace EasyPizza.Application.DTOs;

public class SessionInfoResponse
{
    public Guid SessionId { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhoneNumber { get; set; } = string.Empty;
    public CustomerAddress? DefaultAddress { get; set; }
    public Guid? LastOrderId { get; set; }
    public string? LastOrderSummary { get; set; }
}
