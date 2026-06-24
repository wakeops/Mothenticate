using Microsoft.EntityFrameworkCore;
using Mothenticate.Data.Entities;

namespace Mothenticate.Data.Repositories;

public class UserRepository(MothenticateDbContext db) : IUserRepository
{
    public async Task<ApplicationUser?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        => await db.Users.FindAsync([id], cancellationToken);

    public async Task<ApplicationUser?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
        => await db.Users.FirstOrDefaultAsync(u => u.UserName == username, cancellationToken);

    public async Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        => await db.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    public async Task<IReadOnlyList<ApplicationUser>> GetAllAsync(CancellationToken cancellationToken = default)
        => await db.Users.ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ApplicationUser>> GetByGroupAsync(int groupId, CancellationToken cancellationToken = default)
        => await db.UserGroups
            .Where(ug => ug.GroupId == groupId)
            .Select(ug => ug.User)
            .ToListAsync(cancellationToken);
}
