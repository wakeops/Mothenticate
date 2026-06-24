using Mothenticate.Data.Entities;
using Mothenticate.Data.Repositories;

namespace Mothenticate.UserManagement.Services;

public class AppLauncherService(IAppLauncherRepository appLauncherRepository, IUserService userService) : IAppLauncherService
{
    public Task<IReadOnlyList<AppLauncher>> GetAllAsync(CancellationToken cancellationToken = default)
        => appLauncherRepository.GetAllAsync(cancellationToken);

    public Task<AppLauncher?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => appLauncherRepository.GetByIdAsync(id, cancellationToken);

    public async Task<AppLauncher> CreateAsync(string name, string slug, string? description, string? iconUrl,
        string? launchUrl, string? linkedClientId, bool isEnabled, bool isPublic, CancellationToken cancellationToken = default)
    {
        var app = new AppLauncher
        {
            Name = name,
            Slug = slug,
            Description = description,
            IconUrl = iconUrl,
            LaunchUrl = launchUrl,
            LinkedClientId = linkedClientId,
            IsEnabled = isEnabled,
            IsPublic = isPublic
        };
        return await appLauncherRepository.CreateAsync(app, cancellationToken);
    }

    public async Task UpdateAsync(int id, string name, string slug, string? description, string? iconUrl,
        string? launchUrl, string? linkedClientId, bool isEnabled, bool isPublic, CancellationToken cancellationToken = default)
    {
        var app = await appLauncherRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException($"AppLauncher {id} not found.");

        app.Name = name;
        app.Slug = slug;
        app.Description = description;
        app.IconUrl = iconUrl;
        app.LaunchUrl = launchUrl;
        app.LinkedClientId = linkedClientId;
        app.IsEnabled = isEnabled;
        app.IsPublic = isPublic;

        await appLauncherRepository.UpdateAsync(app, cancellationToken);
    }

    public Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        => appLauncherRepository.DeleteAsync(id, cancellationToken);

    public async Task<IReadOnlyList<AppLauncher>> GetAccessibleForUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        var groups = await userService.GetGroupsAsync(userId, cancellationToken);
        var groupIds = groups.Select(g => g.Id).ToList();
        return await appLauncherRepository.GetAccessibleForUserAsync(userId, groupIds, cancellationToken);
    }

    public Task<IReadOnlyList<AppLauncherAccess>> GetAccessEntriesAsync(int appLauncherId, CancellationToken cancellationToken = default)
        => appLauncherRepository.GetAccessEntriesAsync(appLauncherId, cancellationToken);

    public Task AddGroupAccessAsync(int appLauncherId, int groupId, CancellationToken cancellationToken = default)
        => appLauncherRepository.AddAccessAsync(new AppLauncherAccess { AppLauncherId = appLauncherId, GroupId = groupId }, cancellationToken);

    public Task AddUserAccessAsync(int appLauncherId, string userId, CancellationToken cancellationToken = default)
        => appLauncherRepository.AddAccessAsync(new AppLauncherAccess { AppLauncherId = appLauncherId, UserId = userId }, cancellationToken);

    public Task RemoveAccessAsync(int accessId, CancellationToken cancellationToken = default)
        => appLauncherRepository.RemoveAccessAsync(accessId, cancellationToken);
}
