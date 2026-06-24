using Microsoft.EntityFrameworkCore;
using Mothenticate.Data.Entities;

namespace Mothenticate.Data.Repositories;

public class GroupRepository(MothenticateDbContext db) : IGroupRepository
{
    public async Task<Group?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await db.Groups
            .Include(g => g.GroupRoles).ThenInclude(gr => gr.Role)
            .Include(g => g.UserGroups)
            .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Group>> GetAllAsync(CancellationToken cancellationToken = default)
        => await db.Groups.ToListAsync(cancellationToken);

    public async Task<Group> CreateAsync(Group group, CancellationToken cancellationToken = default)
    {
        db.Groups.Add(group);
        await db.SaveChangesAsync(cancellationToken);
        return group;
    }

    public async Task UpdateAsync(Group group, CancellationToken cancellationToken = default)
    {
        db.Groups.Update(group);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var group = await db.Groups.FindAsync([id], cancellationToken);
        if (group is not null)
        {
            db.Groups.Remove(group);
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task AddMemberAsync(string userId, int groupId, CancellationToken cancellationToken = default)
    {
        var exists = await db.UserGroups.AnyAsync(ug => ug.UserId == userId && ug.GroupId == groupId, cancellationToken);
        if (!exists)
        {
            db.UserGroups.Add(new UserGroup { UserId = userId, GroupId = groupId });
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task RemoveMemberAsync(string userId, int groupId, CancellationToken cancellationToken = default)
    {
        var entry = await db.UserGroups.FirstOrDefaultAsync(ug => ug.UserId == userId && ug.GroupId == groupId, cancellationToken);
        if (entry is not null)
        {
            db.UserGroups.Remove(entry);
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<IReadOnlyList<ApplicationUser>> GetMembersAsync(int groupId, CancellationToken cancellationToken = default)
        => await db.UserGroups
            .Where(ug => ug.GroupId == groupId)
            .Select(ug => ug.User)
            .ToListAsync(cancellationToken);
}
