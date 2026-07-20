using System.Security.Claims;
using Mothenticate.Data.Entities;

namespace Mothenticate.IdentityProvider.Services.ScopeMappers.Abstract;

public interface ITokenMapper : IScopeMapper
{
    Task<List<Claim>> MapTokenAsync(ClaimsIdentity identity, ApplicationUser user, IReadOnlyCollection<UserAttribute> userAttributes, IReadOnlyDictionary<string, string> config, CancellationToken cancellationToken);
}