using System.Security.Claims;
using Mothenticate.Data.Entities;
using Mothenticate.IdentityProvider.Services.ScopeMappers.Abstract;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Mothenticate.IdentityProvider.Services.ScopeMappers.Handlers;

public static class TokenMapperHandler
{
    public static async Task HandleAsync(ITokenMapper mapper, IReadOnlyDictionary<string, string> config, ClaimsIdentity identity, ApplicationUser user, IReadOnlyCollection<UserAttribute> userAttributes,  CancellationToken cancellationToken)
    {
        List<string> destinations = [];

        if (mapper is IAccessTokenMapper && GetConfigFlag(config, "IncludeAccessToken"))
        {
            destinations.Add(Destinations.AccessToken);
        }

        if (mapper is IIdTokenMapper && GetConfigFlag(config, "IncludeIdToken"))
        {
            destinations.Add(Destinations.IdentityToken);
        }

        if (mapper is IIntrospectionTokenMapper && GetConfigFlag(config, "IncludeIntrospectionToken"))
        {
            destinations.Add(Destinations.IssuedToken);
        }

        if (destinations.Count > 0)
        {
            var claims = await mapper.MapTokenAsync(identity, user, userAttributes, config, cancellationToken);
            if (claims == null || claims.Count == 0)
            {
                return;
            }

            foreach (var claim in claims)
            {
                // A claim of this type+value may already be in the identity — either added by an
                // earlier mapper call for the same claim (e.g. another scope also maps "acr"/"sub"),
                // or because the mapper itself returned an existing identity claim. SetDestinations
                // replaces the full destination set, so blindly calling it (and re-adding the claim)
                // would drop whatever destinations an earlier call already set and duplicate the entry.
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

    private static bool GetConfigFlag(IReadOnlyDictionary<string, string> config, string key)
        => config.TryGetValue(key, out var value) && bool.TryParse(value, out var parsed) && parsed;
}