using System.Security.Claims;
using Mothenticate.Data.Entities;
using Mothenticate.IdentityProvider.Services.ScopeMappers.Abstract;
using OpenIddict.Abstractions;

namespace Mothenticate.IdentityProvider.Services.ScopeMappers;

public class SubjectMapper : IAccessTokenMapper, IIntrospectionTokenMapper
{
    public MapperType MapperType => MapperType.Subject;

    private const string _subjectClaimType = OpenIddictConstants.Claims.Subject;

    public async Task<List<Claim>> MapTokenAsync(ClaimsIdentity identity, ApplicationUser user, IReadOnlyCollection<UserAttribute> userAttributes, IReadOnlyDictionary<string, string> config, CancellationToken cancellationToken)
    {    
        var subjectClaim = identity.FindFirst(_subjectClaimType);
        if (subjectClaim == null)
        {
            var subjectValue = user.Id;
            subjectClaim = new Claim(_subjectClaimType, subjectValue);
        } 

        return [subjectClaim];
    }
}