using Mothenticate.Data.Entities;

namespace Mothenticate.Data.Repositories;

public interface IAppSettingsRepository
{
    Task<AppSettings> GetAsync(CancellationToken cancellationToken = default);
    Task UpdateAsync(AppSettings settings, CancellationToken cancellationToken = default);
}
