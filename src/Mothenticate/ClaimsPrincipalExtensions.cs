using System.Security.Claims;
using Mothenticate.Domain.Config;

namespace Mothenticate;

internal static class ClaimsPrincipalExtensions
{
    internal static string? GetUserId(this ClaimsPrincipal principal)
        => principal.FindFirstValue(ClaimTypes.NameIdentifier);

    internal static string? GetDisplayName(this ClaimsPrincipal principal)
        => principal.FindFirstValue(AuthDefaults.PreferredUsernameClaimType) ?? principal.FindFirstValue(ClaimTypes.Name);
}
