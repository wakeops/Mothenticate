using Mothenticate.Data.Entities;

namespace Mothenticate.UserManagement.Services;

public interface IAppSettingsService
{
    Task<AppSettings> GetAsync(CancellationToken cancellationToken = default);
    Task UpdateAsync(AppSettings settings, CancellationToken cancellationToken = default);
}
