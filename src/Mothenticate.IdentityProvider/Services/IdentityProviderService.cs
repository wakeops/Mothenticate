using System.Text.Json;
using Mothenticate.Data.Entities;
using Mothenticate.Data.Repositories;
using IdpEntity = Mothenticate.Data.Entities.IdentityProvider;

namespace Mothenticate.IdentityProvider.Services;

public class IdentityProviderService(IIdentityProviderRepository repository) : IIdentityProviderService
{
    private static readonly (string Name, ProtocolType Protocol, string? DefaultProperties)[] _typeSeed =
    [
        ("OIDC", ProtocolType.OIDC, null),
        ("OAuth2", ProtocolType.OAuth2, null),
        ("SAML 2.0", ProtocolType.SAMLv2, null),
        ("Google", ProtocolType.OIDC, JsonSerializer.Serialize(new Dictionary<string, string>
        {
            ["discovery_url"] = "https://accounts.google.com/.well-known/openid-configuration",
            ["authorization_endpoint"] = "https://accounts.google.com/o/oauth2/v2/auth",
            ["token_endpoint"] = "https://oauth2.googleapis.com/token",
            ["userinfo_endpoint"] = "https://openidconnect.googleapis.com/v1/userinfo",
        })),
        ("GitHub", ProtocolType.OAuth2, JsonSerializer.Serialize(new Dictionary<string, string>
        {
            ["authorization_endpoint"] = "https://github.com/login/oauth/authorize",
            ["token_endpoint"] = "https://github.com/login/oauth/access_token",
            ["userinfo_endpoint"] = "https://api.github.com/user",
            ["default_scopes"] = "user:email",
        })),
    ];

    public async Task SeedProviderTypesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var (name, protocol, defaultProps) in _typeSeed)
            await repository.UpsertProviderTypeAsync(name, protocol, defaultProps, cancellationToken);
    }

    public Task<IReadOnlyList<IdentityProviderType>> GetProviderTypesAsync(CancellationToken cancellationToken = default)
        => repository.GetProviderTypesAsync(cancellationToken);

    public Task<IReadOnlyList<IdpEntity>> GetProvidersAsync(CancellationToken cancellationToken = default)
        => repository.GetProvidersAsync(cancellationToken);

    public Task<IdpEntity?> GetProviderAsync(int id, CancellationToken cancellationToken = default)
        => repository.GetProviderAsync(id, cancellationToken);

    public Task<IdpEntity?> GetProviderByAliasAsync(string alias, CancellationToken cancellationToken = default)
        => repository.GetProviderByAliasAsync(alias, cancellationToken);

    public Task<IdpEntity> CreateProviderAsync(IdpEntity provider, CancellationToken cancellationToken = default)
        => repository.CreateProviderAsync(provider, cancellationToken);

    public Task UpdateProviderAsync(IdpEntity provider, CancellationToken cancellationToken = default)
        => repository.UpdateProviderAsync(provider, cancellationToken);

    public Task DeleteProviderAsync(int id, CancellationToken cancellationToken = default)
        => repository.DeleteProviderAsync(id, cancellationToken);

    public Task SetProviderEnabledAsync(int id, bool enabled, CancellationToken cancellationToken = default)
        => repository.SetProviderEnabledAsync(id, enabled, cancellationToken);

    public Task<IdentityProviderMapper> AddMapperAsync(IdentityProviderMapper mapper, CancellationToken cancellationToken = default)
        => repository.AddMapperAsync(mapper, cancellationToken);

    public Task UpdateMapperAsync(IdentityProviderMapper mapper, CancellationToken cancellationToken = default)
        => repository.UpdateMapperAsync(mapper, cancellationToken);

    public Task DeleteMapperAsync(int mapperId, CancellationToken cancellationToken = default)
        => repository.DeleteMapperAsync(mapperId, cancellationToken);
}
