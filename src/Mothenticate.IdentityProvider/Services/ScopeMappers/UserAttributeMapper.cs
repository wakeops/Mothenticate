using System.Security.Claims;
using Mothenticate.Data.Entities;
using Mothenticate.IdentityProvider.Services.ScopeMappers.Abstract;
using Mothenticate.IdentityProvider.Services.ScopeMappers.MapperConfigs;

namespace Mothenticate.IdentityProvider.Services.ScopeMappers;

public class UserAttributeMapper : ConfigurableMapper<UserAttributeMapperConfig>, IAccessTokenMapper, IIdTokenMapper, IIntrospectionTokenMapper, IUserInfoMapper
{
    public MapperType MapperType => MapperType.UserAttribute;

    public async Task<List<Claim>> MapTokenAsync(ClaimsIdentity identity, ApplicationUser user, IReadOnlyCollection<UserAttribute> userAttributes, IReadOnlyDictionary<string, string> config, CancellationToken cancellationToken)
    {
        var settings = GetSettingsFromConfig(config);
        var value = GetValue(identity, userAttributes, settings);

        if (value == null)
        {
            return [];
        }

        return [new Claim(settings.TokenClaimName!, value)];
    }

    public async Task<Dictionary<string, string>> MapUserInfoAsync(ClaimsIdentity identity, ApplicationUser user, IReadOnlyCollection<UserAttribute> userAttributes, IReadOnlyDictionary<string, string> config, CancellationToken cancellationToken)
    {
        var settings = GetSettingsFromConfig(config);
        var value = GetValue(identity, userAttributes, settings);

        if (value == null)
        {
            return [];
        }

        return new Dictionary<string, string>
        {
            [settings.TokenClaimName!] = value,
        };
    }

    private static string? GetValue(ClaimsIdentity identity, IReadOnlyCollection<UserAttribute> userAttributes, UserAttributeMapperConfig settings)
    {
        if (settings.UserAttributeId == null || string.IsNullOrEmpty(settings.TokenClaimName))
        {
            return null;
        }

        var identityClaim = identity.FindFirst(settings.TokenClaimName);
        if (identityClaim != null)
        {
            return identityClaim.Value;
        } 

        var userAttribute = userAttributes.FirstOrDefault(attr => settings.UserAttributeId == attr.Id);
        if (userAttribute == null)
        {
            return null;
        }

        if (userAttribute.Values.Count == 0 && !string.IsNullOrEmpty(userAttribute.DefaultValue))
        {
            return userAttribute.DefaultValue!;
        }

        if (userAttribute.Values.Count == 0)
        {
            return null;
        }

        return string.Join(", ", userAttribute.Values
            .OrderBy(v => v.Ordinal)
            .Select(v => v.Value)
            .Where(v => !string.IsNullOrEmpty(v)));
    }
}