using Mothenticate.IdentityProvider.Models;

namespace Mothenticate.IdentityProvider.Services;

public interface IConsentService
{
    Task<IReadOnlyList<ConsentInfo>> GetBySubjectAsync(string userId, CancellationToken cancellationToken = default);
    Task RevokeAsync(string authorizationId, CancellationToken cancellationToken = default);
    Task RevokeAllBySubjectAsync(string userId, CancellationToken cancellationToken = default);
}
