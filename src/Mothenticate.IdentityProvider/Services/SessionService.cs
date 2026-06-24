using Mothenticate.Data.Entities;
using Mothenticate.Data.Repositories;

namespace Mothenticate.IdentityProvider.Services;

public class SessionService(ISessionRepository sessionRepository) : ISessionService
{
    public Task<UserSession> CreateAsync(string userId, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default)
        => sessionRepository.CreateAsync(new UserSession
        {
            UserId = userId,
            IpAddress = ipAddress,
            UserAgent = userAgent
        }, cancellationToken);

    public Task<UserSession?> GetByIdAsync(Guid sessionId, CancellationToken cancellationToken = default)
        => sessionRepository.GetByIdAsync(sessionId, cancellationToken);

    public Task<IReadOnlyList<UserSession>> GetActiveByUserIdAsync(string userId, CancellationToken cancellationToken = default)
        => sessionRepository.GetActiveByUserIdAsync(userId, cancellationToken);

    public Task RevokeAsync(Guid sessionId, CancellationToken cancellationToken = default)
        => sessionRepository.RevokeAsync(sessionId, cancellationToken);

    public Task RevokeAllForUserAsync(string userId, CancellationToken cancellationToken = default)
        => sessionRepository.RevokeAllForUserAsync(userId, cancellationToken);
}
