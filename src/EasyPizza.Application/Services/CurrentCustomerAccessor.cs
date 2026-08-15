using EasyPizza.Application.Interfaces.Services;

namespace EasyPizza.Application.Services;

public class CurrentCustomerAccessor : ICurrentCustomerAccessor
{
    public Guid? CustomerId { get; set; }
    public Guid? SessionId { get; set; }
}
