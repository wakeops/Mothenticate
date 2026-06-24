using Microsoft.EntityFrameworkCore;
using Mothenticate.Data.Entities;

namespace Mothenticate.Data.Repositories;

public class RoleRepository(MothenticateDbContext db) : IRoleRepository
{
    public async Task<AppRole?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await db.AppRoles.FindAsync([id], cancellationToken);

    public async Task<IReadOnlyList<AppRole>> GetAllAsync(CancellationToken cancellationToken = default)
        => await db.AppRoles.ToListAsync(cancellationToken);

    public async Task<AppRole> CreateAsync(AppRole role, CancellationToken cancellationToken = default)
    {
        db.AppRoles.Add(role);
        await db.SaveChangesAsync(cancellationToken);
        return role;
    }

    public async Task UpdateAsync(AppRole role, CancellationToken cancellationToken = default)
    {
        db.AppRoles.Update(role);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var role = await db.AppRoles.FindAsync([id], cancellationToken);
        if (role is not null)
        {
            db.AppRoles.Remove(role);
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task AddToGroupAsync(int roleId, int groupId, CancellationToken cancellationToken = default)
    {
        var exists = await db.GroupRoles.AnyAsync(gr => gr.RoleId == roleId && gr.GroupId == groupId, cancellationToken);
        if (!exists)
        {
            db.GroupRoles.Add(new GroupRole { RoleId = roleId, GroupId = groupId });
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task RemoveFromGroupAsync(int roleId, int groupId, CancellationToken cancellationToken = default)
    {
        var entry = await db.GroupRoles.FirstOrDefaultAsync(gr => gr.RoleId == roleId && gr.GroupId == groupId, cancellationToken);
        if (entry is not null)
        {
            db.GroupRoles.Remove(entry);
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<IReadOnlyList<AppRole>> GetByGroupAsync(int groupId, CancellationToken cancellationToken = default)
        => await db.GroupRoles
            .Where(gr => gr.GroupId == groupId)
            .Select(gr => gr.Role)
            .ToListAsync(cancellationToken);
}
