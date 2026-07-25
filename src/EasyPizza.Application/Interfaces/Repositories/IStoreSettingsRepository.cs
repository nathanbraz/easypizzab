using EasyPizza.Domain.Entities;

namespace EasyPizza.Application.Interfaces.Repositories;

public interface IStoreSettingsRepository
{
    Task<StoreSettings> GetSettingsAsync();
    Task UpdateAsync(StoreSettings settings);
}
