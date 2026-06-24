using Mothenticate.Data.Entities;
using Mothenticate.Data.Repositories;

namespace Mothenticate.UserManagement.Services;

public class AppSettingsService(IAppSettingsRepository repository) : IAppSettingsService
{
    public Task<AppSettings> GetAsync(CancellationToken cancellationToken = default)
        => repository.GetAsync(cancellationToken);

    public Task UpdateAsync(AppSettings settings, CancellationToken cancellationToken = default)
        => repository.UpdateAsync(settings, cancellationToken);
}
