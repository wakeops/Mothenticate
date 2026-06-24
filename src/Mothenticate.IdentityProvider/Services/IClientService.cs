using Mothenticate.IdentityProvider.Models;
using OpenIddict.Abstractions;

namespace Mothenticate.IdentityProvider.Services;

public interface IClientService
{
    Task<IReadOnlyList<ClientInfo>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ClientInfo?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<ClientInfo?> GetByClientIdAsync(string clientId, CancellationToken cancellationToken = default);
    Task<string?> CreateAsync(OpenIddictApplicationDescriptor descriptor, CancellationToken cancellationToken = default);
    Task UpdateAsync(string id, OpenIddictApplicationDescriptor descriptor, CancellationToken cancellationToken = default);
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);

    // Enabled flag
    Task SetEnabledAsync(string applicationId, bool isEnabled, CancellationToken cancellationToken = default);

    // Secret management
    Task<IReadOnlyList<ClientSecretInfo>> GetSecretsAsync(string applicationId, CancellationToken cancellationToken = default);
    Task<int> AddSecretAsync(string applicationId, string secretValue, string? description = null, DateTimeOffset? expiresAt = null, CancellationToken cancellationToken = default);
    Task RemoveSecretAsync(int secretId, CancellationToken cancellationToken = default);
    Task<bool> ValidateSecretAsync(string applicationId, string secretValue, CancellationToken cancellationToken = default);
}
