using EasyPizza.Domain.Entities;

namespace EasyPizza.Application.Interfaces.Repositories;

public interface ICourierRepository : IRepository<Courier>
{
    Task<IEnumerable<Courier>> GetActiveCouriersAsync();
}
