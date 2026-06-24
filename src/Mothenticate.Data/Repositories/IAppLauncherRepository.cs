using Mothenticate.Data.Entities;

namespace Mothenticate.Data.Repositories;

public interface IAppLauncherRepository
{
    Task<AppLauncher?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<AppLauncher?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AppLauncher>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AppLauncher>> GetAccessibleForUserAsync(string userId, IReadOnlyList<int> groupIds, CancellationToken cancellationToken = default);
    Task<AppLauncher> CreateAsync(AppLauncher appLauncher, CancellationToken cancellationToken = default);
    Task UpdateAsync(AppLauncher appLauncher, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AppLauncherAccess>> GetAccessEntriesAsync(int appLauncherId, CancellationToken cancellationToken = default);
    Task AddAccessAsync(AppLauncherAccess access, CancellationToken cancellationToken = default);
    Task RemoveAccessAsync(int accessId, CancellationToken cancellationToken = default);
}
