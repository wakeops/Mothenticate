using Mothenticate.Data.Entities;

namespace Mothenticate.Data.Repositories;

public interface IIdentityProviderRepository
{
    Task<IReadOnlyList<IdentityProviderType>> GetProviderTypesAsync(CancellationToken cancellationToken = default);
    Task UpsertProviderTypeAsync(string name, ProtocolType protocolType, string? defaultProperties, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<IdentityProvider>> GetProvidersAsync(CancellationToken cancellationToken = default);
    Task<IdentityProvider?> GetProviderAsync(int id, CancellationToken cancellationToken = default);
    Task<IdentityProvider?> GetProviderByAliasAsync(string alias, CancellationToken cancellationToken = default);
    Task<IdentityProvider> CreateProviderAsync(IdentityProvider provider, CancellationToken cancellationToken = default);
    Task UpdateProviderAsync(IdentityProvider provider, CancellationToken cancellationToken = default);
    Task DeleteProviderAsync(int id, CancellationToken cancellationToken = default);
    Task SetProviderEnabledAsync(int id, bool enabled, CancellationToken cancellationToken = default);

    Task<IdentityProviderMapper> AddMapperAsync(IdentityProviderMapper mapper, CancellationToken cancellationToken = default);
    Task UpdateMapperAsync(IdentityProviderMapper mapper, CancellationToken cancellationToken = default);
    Task DeleteMapperAsync(int mapperId, CancellationToken cancellationToken = default);
}
