using Mothenticate.Data.Entities;

namespace Mothenticate.Data.Repositories;

public interface ISessionRepository
{
    Task<UserSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserSession>> GetActiveByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<UserSession> CreateAsync(UserSession session, CancellationToken cancellationToken = default);
    Task RevokeAsync(Guid id, CancellationToken cancellationToken = default);
    Task RevokeAllForUserAsync(string userId, CancellationToken cancellationToken = default);
}
