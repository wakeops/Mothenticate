using System.Security.Claims;
using Mothenticate.Data.Entities;
using Mothenticate.IdentityProvider.Services.ScopeMappers.Abstract;
using OpenIddict.Abstractions;

namespace Mothenticate.IdentityProvider.Services.ScopeMappers;

public class AcrMapper : IAccessTokenMapper, IIdTokenMapper, IIntrospectionTokenMapper
{
    public MapperType MapperType => MapperType.AuthenticationContextReference;

    private const string _acrClaimType = OpenIddictConstants.Claims.AuthenticationContextReference;

    public async Task<List<Claim>> MapTokenAsync(ClaimsIdentity identity, ApplicationUser user, IReadOnlyCollection<UserAttribute> userAttributes, IReadOnlyDictionary<string, string> config, CancellationToken cancellationToken)
    {    
        var acrClaim = identity.FindFirst(_acrClaimType);
        if (acrClaim == null)
        {
            var acrValue = user.TwoFactorEnabled ? "urn:mothenticate:loa:2" : "urn:mothenticate:loa:1";
            acrClaim = new Claim(_acrClaimType, acrValue);
        } 

        return [acrClaim];
    }
}