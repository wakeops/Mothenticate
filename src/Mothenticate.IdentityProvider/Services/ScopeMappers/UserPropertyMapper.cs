using System.Security.Claims;
using Mothenticate.Data.Entities;
using Mothenticate.IdentityProvider.Services.ScopeMappers.Abstract;
using Mothenticate.IdentityProvider.Services.ScopeMappers.MapperConfigs;

namespace Mothenticate.IdentityProvider.Services.ScopeMappers;

public class UserPropertyMapper : ConfigurableMapper<UserPropertyMapperConfig>, IAccessTokenMapper, IIdTokenMapper, IIntrospectionTokenMapper, IUserInfoMapper
{
    public MapperType MapperType => MapperType.UserProperty;

    public async Task<List<Claim>> MapTokenAsync(ClaimsIdentity identity, ApplicationUser user, IReadOnlyCollection<UserAttribute> userAttributes, IReadOnlyDictionary<string, string> config, CancellationToken cancellationToken)
    {
        var settings = GetSettingsFromConfig(config);
        var value = GetValue(identity, user, settings);

        if (value == null)
        {
            return [];
        }

        return [new Claim(settings.TokenClaimName!, value)];
    }

    public async Task<Dictionary<string, string>> MapUserInfoAsync(ClaimsIdentity identity, ApplicationUser user, IReadOnlyCollection<UserAttribute> userAttributes, IReadOnlyDictionary<string, string> config, CancellationToken cancellationToken)
    {
        var settings = GetSettingsFromConfig(config);
        var value = GetValue(identity, user, settings);

        if (value == null)
        {
            return [];
        }

        return new Dictionary<string, string>
        {
            [settings.TokenClaimName!] = value,
        };
    }

    private static string? GetValue(ClaimsIdentity identity, ApplicationUser user, UserPropertyMapperConfig settings)
    {
        if (settings.UserProperty is null || string.IsNullOrEmpty(settings.TokenClaimName))
        {
            return null;
        }

        var identityClaim = identity.FindFirst(settings.TokenClaimName);
        if (identityClaim != null)
        {
            return identityClaim.Value;
        }

        return settings.UserProperty switch
        {
            UserPropertyField.EmailConfirmed => user.EmailConfirmed.ToString().ToLowerInvariant(),
            _ => null
        };
    }
}