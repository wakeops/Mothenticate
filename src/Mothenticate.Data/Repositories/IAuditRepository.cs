using Mothenticate.Data.Entities;

namespace Mothenticate.Data.Repositories;

public interface IAuditRepository
{
    Task LogAsync(AuditLog entry, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuditLog>> GetByResourceAsync(string resourceType, string resourceId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuditLog>> GetByActorAsync(string actorUserId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuditLog>> GetRecentAsync(int count = 50, CancellationToken cancellationToken = default);
}
