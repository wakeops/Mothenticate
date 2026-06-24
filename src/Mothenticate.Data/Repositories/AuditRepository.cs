using Microsoft.EntityFrameworkCore;
using Mothenticate.Data.Entities;

namespace Mothenticate.Data.Repositories;

public class AuditRepository(MothenticateDbContext db) : IAuditRepository
{
    public async Task LogAsync(AuditLog entry, CancellationToken cancellationToken = default)
    {
        db.AuditLogs.Add(entry);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AuditLog>> GetByResourceAsync(string resourceType, string resourceId, CancellationToken cancellationToken = default)
        => await db.AuditLogs
            .Where(a => a.ResourceType == resourceType && a.ResourceId == resourceId)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<AuditLog>> GetByActorAsync(string actorUserId, CancellationToken cancellationToken = default)
        => await db.AuditLogs
            .Where(a => a.ActorUserId == actorUserId)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<AuditLog>> GetRecentAsync(int count = 50, CancellationToken cancellationToken = default)
        => await db.AuditLogs
            .OrderByDescending(a => a.Timestamp)
            .Take(count)
            .ToListAsync(cancellationToken);
}
