using Mothenticate.Data.Entities;

namespace Mothenticate.IdentityProvider.Services;

public interface ISessionService
{
    Task<UserSession> CreateAsync(string userId, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default);
    Task<UserSession?> GetByIdAsync(Guid sessionId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserSession>> GetActiveByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task RevokeAsync(Guid sessionId, CancellationToken cancellationToken = default);
    Task RevokeAllForUserAsync(string userId, CancellationToken cancellationToken = default);
}
