using Microsoft.EntityFrameworkCore;
using Mothenticate.Data.Entities;

namespace Mothenticate.Data.Repositories;

public class AppLauncherRepository(MothenticateDbContext db) : IAppLauncherRepository
{
    public async Task<AppLauncher?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await db.AppLaunchers
            .Include(a => a.AccessEntries).ThenInclude(e => e.User)
            .Include(a => a.AccessEntries).ThenInclude(e => e.Group)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task<AppLauncher?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
        => await db.AppLaunchers
            .FirstOrDefaultAsync(a => a.Slug == slug, cancellationToken);

    public async Task<IReadOnlyList<AppLauncher>> GetAllAsync(CancellationToken cancellationToken = default)
        => await db.AppLaunchers
            .Include(a => a.AccessEntries)
            .OrderBy(a => a.Name)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<AppLauncher>> GetAccessibleForUserAsync(
        string userId, IReadOnlyList<int> groupIds, CancellationToken cancellationToken = default)
        => await db.AppLaunchers
            .Where(a => a.IsEnabled && (
                a.IsPublic
                || a.AccessEntries.Any(e => e.UserId == userId)
                || a.AccessEntries.Any(e => e.GroupId.HasValue && groupIds.Contains(e.GroupId.Value))))
            .OrderBy(a => a.Name)
            .ToListAsync(cancellationToken);

    public async Task<AppLauncher> CreateAsync(AppLauncher appLauncher, CancellationToken cancellationToken = default)
    {
        db.AppLaunchers.Add(appLauncher);
        await db.SaveChangesAsync(cancellationToken);
        return appLauncher;
    }

    public async Task UpdateAsync(AppLauncher appLauncher, CancellationToken cancellationToken = default)
    {
        db.AppLaunchers.Update(appLauncher);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var app = await db.AppLaunchers.FindAsync([id], cancellationToken);
        if (app is not null)
        {
            db.AppLaunchers.Remove(app);
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<IReadOnlyList<AppLauncherAccess>> GetAccessEntriesAsync(int appLauncherId, CancellationToken cancellationToken = default)
        => await db.AppLauncherAccess
            .Include(e => e.User)
            .Include(e => e.Group)
            .Where(e => e.AppLauncherId == appLauncherId)
            .ToListAsync(cancellationToken);

    public async Task AddAccessAsync(AppLauncherAccess access, CancellationToken cancellationToken = default)
    {
        db.AppLauncherAccess.Add(access);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveAccessAsync(int accessId, CancellationToken cancellationToken = default)
    {
        var entry = await db.AppLauncherAccess.FindAsync([accessId], cancellationToken);
        if (entry is not null)
        {
            db.AppLauncherAccess.Remove(entry);
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
