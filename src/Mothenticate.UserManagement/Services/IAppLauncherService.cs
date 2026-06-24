using Mothenticate.Data.Entities;

namespace Mothenticate.UserManagement.Services;

public interface IAppLauncherService
{
    Task<IReadOnlyList<AppLauncher>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<AppLauncher?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<AppLauncher> CreateAsync(string name, string slug, string? description, string? iconUrl, string? launchUrl, string? linkedClientId, bool isEnabled, bool isPublic, CancellationToken cancellationToken = default);
    Task UpdateAsync(int id, string name, string slug, string? description, string? iconUrl, string? launchUrl, string? linkedClientId, bool isEnabled, bool isPublic, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AppLauncher>> GetAccessibleForUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AppLauncherAccess>> GetAccessEntriesAsync(int appLauncherId, CancellationToken cancellationToken = default);
    Task AddGroupAccessAsync(int appLauncherId, int groupId, CancellationToken cancellationToken = default);
    Task AddUserAccessAsync(int appLauncherId, string userId, CancellationToken cancellationToken = default);
    Task RemoveAccessAsync(int accessId, CancellationToken cancellationToken = default);
}
