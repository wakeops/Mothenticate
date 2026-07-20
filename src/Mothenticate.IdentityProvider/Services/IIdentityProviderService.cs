using Mothenticate.Data.Entities;
using IdpEntity = Mothenticate.Data.Entities.IdentityProvider;

namespace Mothenticate.IdentityProvider.Services;

public interface IIdentityProviderService
{
    Task SeedProviderTypesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<IdentityProviderType>> GetProviderTypesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<IdpEntity>> GetProvidersAsync(CancellationToken cancellationToken = default);
    Task<IdpEntity?> GetProviderAsync(int id, CancellationToken cancellationToken = default);
    Task<IdpEntity?> GetProviderByAliasAsync(string alias, CancellationToken cancellationToken = default);
    Task<IdpEntity> CreateProviderAsync(IdpEntity provider, CancellationToken cancellationToken = default);
    Task UpdateProviderAsync(IdpEntity provider, CancellationToken cancellationToken = default);
    Task DeleteProviderAsync(int id, CancellationToken cancellationToken = default);
    Task SetProviderEnabledAsync(int id, bool enabled, CancellationToken cancellationToken = default);

    Task<IdentityProviderMapper> AddMapperAsync(IdentityProviderMapper mapper, CancellationToken cancellationToken = default);
    Task UpdateMapperAsync(IdentityProviderMapper mapper, CancellationToken cancellationToken = default);
    Task DeleteMapperAsync(int mapperId, CancellationToken cancellationToken = default);
}
