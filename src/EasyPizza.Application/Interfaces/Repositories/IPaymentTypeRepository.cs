using EasyPizza.Domain.Entities;

namespace EasyPizza.Application.Interfaces.Repositories;

public interface IPaymentTypeRepository
{
    Task<PaymentType?> GetByIdAsync(Guid id);
    Task<IEnumerable<PaymentType>> GetAllActiveAsync();
}
