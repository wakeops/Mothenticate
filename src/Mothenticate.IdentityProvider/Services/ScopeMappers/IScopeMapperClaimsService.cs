using System.Security.Claims;
using Mothenticate.Data.Entities;

namespace Mothenticate.IdentityProvider.Services.ScopeMappers;

/// <summary>
/// Single entry point for applying configured scope-mapper claims to the three OIDC destinations that
/// consume them (token issuance, userinfo, introspection), so each destination doesn't independently
/// re-fetch mapper rows and re-implement mapper resolution/config parsing.
/// </summary>
public interface IScopeMapperClaimsService
{
    Task ApplyTokenClaimsAsync(ClaimsIdentity identity, ApplicationUser user, IReadOnlyList<string> scopes, CancellationToken cancellationToken = default);

    Task<Dictionary<string, string>> GetUserInfoClaimsAsync(ClaimsIdentity identity, ApplicationUser user, IReadOnlyList<string> scopes, CancellationToken cancellationToken = default);

    Task<Dictionary<string, string>> GetIntrospectionClaimsAsync(ApplicationUser user, IReadOnlyList<string> scopes, CancellationToken cancellationToken = default);
}
