using Microsoft.EntityFrameworkCore;
using Mothenticate.Data.Entities;

namespace Mothenticate.Data.Repositories;

public class SessionRepository(MothenticateDbContext db) : ISessionRepository
{
    public async Task<UserSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await db.UserSessions.FindAsync([id], cancellationToken);

    public async Task<IReadOnlyList<UserSession>> GetActiveByUserIdAsync(string userId, CancellationToken cancellationToken = default)
        => await db.UserSessions
            .Where(s => s.UserId == userId && s.RevokedAt == null)
            .OrderByDescending(s => s.LastActiveAt)
            .ToListAsync(cancellationToken);

    public async Task<UserSession> CreateAsync(UserSession session, CancellationToken cancellationToken = default)
    {
        db.UserSessions.Add(session);
        await db.SaveChangesAsync(cancellationToken);
        return session;
    }

    public async Task RevokeAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await db.UserSessions
            .Where(s => s.Id == id && s.RevokedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.RevokedAt, DateTimeOffset.UtcNow), cancellationToken);
    }

    public async Task RevokeAllForUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        await db.UserSessions
            .Where(s => s.UserId == userId && s.RevokedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.RevokedAt, DateTimeOffset.UtcNow), cancellationToken);
    }
}
