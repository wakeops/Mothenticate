using Microsoft.EntityFrameworkCore;
using Mothenticate.Data.Entities;
using Mothenticate.Data.Services;

namespace Mothenticate.Data.Repositories;

public class UserRepository(MothenticateDbContext db) : IUserRepository
{
    public async Task<ApplicationUser?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var user = await db.Users.FindAsync([id], cancellationToken);
        await ApplicationUserHydrator.HydrateAsync(db, user, cancellationToken);
        return user;
    }

    public async Task<ApplicationUser?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        var normalized = username.ToUpperInvariant();
        var userId = await db.UserAttributeValues
            .Where(v => v.UserAttribute.Name == "username" && v.Value != null && v.Value.ToUpper() == normalized)
            .Select(v => v.UserId)
            .FirstOrDefaultAsync(cancellationToken);

        return userId is null ? null : await GetByIdAsync(userId, cancellationToken);
    }

    public async Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalized = email.ToUpperInvariant();
        var userId = await db.UserAttributeValues
            .Where(v => v.UserAttribute.Name == "email" && v.Value != null && v.Value.ToUpper() == normalized)
            .Select(v => v.UserId)
            .FirstOrDefaultAsync(cancellationToken);

        return userId is null ? null : await GetByIdAsync(userId, cancellationToken);
    }

    public async Task<IReadOnlyList<ApplicationUser>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = await db.Users.ToListAsync(cancellationToken);
        await ApplicationUserHydrator.HydrateManyAsync(db, users, cancellationToken);
        return users;
    }

    public async Task<IReadOnlyList<ApplicationUser>> GetByGroupAsync(int groupId, CancellationToken cancellationToken = default)
    {
        var users = await db.UserGroups
            .Where(ug => ug.GroupId == groupId)
            .Select(ug => ug.User)
            .ToListAsync(cancellationToken);
        await ApplicationUserHydrator.HydrateManyAsync(db, users, cancellationToken);
        return users;
    }
}
