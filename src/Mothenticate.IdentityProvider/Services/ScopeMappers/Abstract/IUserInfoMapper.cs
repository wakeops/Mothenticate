using System.Security.Claims;
using Mothenticate.Data.Entities;

namespace Mothenticate.IdentityProvider.Services.ScopeMappers.Abstract;

public interface IUserInfoMapper
{
    Task<Dictionary<string, string>> MapUserInfoAsync(ClaimsIdentity identity, ApplicationUser user, IReadOnlyCollection<UserAttribute> userAttributes, IReadOnlyDictionary<string, string> config, CancellationToken cancellationToken);
}