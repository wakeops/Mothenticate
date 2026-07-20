using System.Security.Claims;
using Mothenticate.Data.Entities;
using Mothenticate.IdentityProvider.Services.ScopeMappers.Abstract;
using Mothenticate.UserManagement.Services;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Mothenticate.IdentityProvider.Services.ScopeMappers;

public class ScopeMapperClaimsService(
    IClientScopeService clientScopeService,
    IUserAttributeService userAttributeService,
    IScopeMapperResolver scopeMapperResolver) : IScopeMapperClaimsService
{
    public async Task ApplyTokenClaimsAsync(ClaimsIdentity identity, ApplicationUser user, IReadOnlyList<string> scopes, CancellationToken cancellationToken = default)
    {
        var mapperRows = await clientScopeService.GetMappersByScopeNamesAsync(scopes, cancellationToken);
        if (mapperRows.Count == 0)
        {
            return;
        }

        var userAttributes = await userAttributeService.GetAllWithUserValuesAsync(user.Id, cancellationToken);

        foreach (var mapperRow in mapperRows)
        {
            if (scopeMapperResolver.Resolve(mapperRow.MapperType) is not ITokenMapper tokenMapper)
            {
                continue;
            }

            List<string> destinations = [];
            if (tokenMapper is IAccessTokenMapper && mapperRow.IncludeAccessToken)
            {
                destinations.Add(Destinations.AccessToken);
            }

            if (tokenMapper is IIdTokenMapper && mapperRow.IncludeIdToken)
            {
                destinations.Add(Destinations.IdentityToken);
            }

            if (destinations.Count == 0)
            {
                continue;
            }

            var config = MapperConfigSerializer.Deserialize(mapperRow.Config);
            var claims = await tokenMapper.MapTokenAsync(identity, user, userAttributes, config, cancellationToken);
            ApplyDestinations(identity, claims, destinations);
        }
    }

    public async Task<Dictionary<string, string>> GetUserInfoClaimsAsync(ClaimsIdentity identity, ApplicationUser user, IReadOnlyList<string> scopes, CancellationToken cancellationToken = default)
    {
        var userInfo = new Dictionary<string, string>();

        var mapperRows = await clientScopeService.GetMappersByScopeNamesAsync(scopes, cancellationToken);
        if (mapperRows.Count == 0)
        {
            return userInfo;
        }

        var userAttributes = await userAttributeService.GetAllWithUserValuesAsync(user.Id, cancellationToken);

        foreach (var mapperRow in mapperRows)
        {
            if (!mapperRow.IncludeUserInfo)
            {
                continue;
            }

            if (scopeMapperResolver.Resolve(mapperRow.MapperType) is not IUserInfoMapper userInfoMapper)
            {
                continue;
            }

            var config = MapperConfigSerializer.Deserialize(mapperRow.Config);
            var claims = await userInfoMapper.MapUserInfoAsync(identity, user, userAttributes, config, cancellationToken);
            foreach (var (key, value) in claims)
            {
                userInfo[key] = value;
            }
        }

        return userInfo;
    }

    public async Task<Dictionary<string, string>> GetIntrospectionClaimsAsync(ApplicationUser user, IReadOnlyList<string> scopes, CancellationToken cancellationToken = default)
    {
        var claims = new Dictionary<string, string>();

        var mapperRows = await clientScopeService.GetMappersByScopeNamesAsync(scopes, cancellationToken);
        if (mapperRows.Count == 0)
        {
            return claims;
        }

        var userAttributes = await userAttributeService.GetAllWithUserValuesAsync(user.Id, cancellationToken);
        var identity = new ClaimsIdentity();

        foreach (var mapperRow in mapperRows)
        {
            if (!mapperRow.IncludeIntrospectionToken)
            {
                continue;
            }

            if (scopeMapperResolver.Resolve(mapperRow.MapperType) is not IIntrospectionTokenMapper introspectionMapper)
            {
                continue;
            }

            var config = MapperConfigSerializer.Deserialize(mapperRow.Config);
            var mappedClaims = await introspectionMapper.MapTokenAsync(identity, user, userAttributes, config, cancellationToken);
            foreach (var claim in mappedClaims)
            {
                claims[claim.Type] = claim.Value;
            }
        }

        return claims;
    }

    // A claim of this type+value may already be on the identity — either added by an earlier mapper
    // call for the same claim (e.g. another scope also maps "acr"/"sub"), or because the mapper itself
    // returned an existing identity claim. SetDestinations replaces the full destination set, so blindly
    // calling it (and re-adding the claim) would drop whatever destinations an earlier call already set
    // and duplicate the entry.
    private static void ApplyDestinations(ClaimsIdentity identity, List<Claim> claims, List<string> destinations)
    {
        foreach (var claim in claims)
        {
            var existingClaim = identity.FindFirst(c => c.Type == claim.Type && c.Value == claim.Value);
            if (existingClaim is not null)
            {
                existingClaim.SetDestinations(existingClaim.GetDestinations().Union(destinations, StringComparer.OrdinalIgnoreCase));
                continue;
            }

            claim.SetDestinations(destinations);
            identity.AddClaim(claim);
        }
    }
}
