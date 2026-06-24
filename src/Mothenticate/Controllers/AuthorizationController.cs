using System.Security.Claims;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Mothenticate.Data.Entities;
using Mothenticate.Domain.Config;
using Mothenticate.IdentityProvider.Services;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

using Destinations = OpenIddict.Abstractions.OpenIddictConstants.Destinations;
using Scopes = OpenIddict.Abstractions.OpenIddictConstants.Scopes;

namespace Mothenticate.Controllers;

using static AuthErrorCodes;

public class AuthorizationController(
    UserManager<ApplicationUser> userManager,
    IOpenIddictApplicationManager applicationManager,
    IOpenIddictAuthorizationManager authorizationManager,
    IClientService clientService) : Controller
{
    // ── Authorization endpoint ────────────────────────────────────────────────

    [HttpGet("~/connect/authorize")]
    [HttpPost("~/connect/authorize")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Authorize()
    {
        var request = HttpContext.GetOpenIddictServerRequest()
            ?? throw new InvalidOperationException("OpenIddict request cannot be retrieved.");

        var result = await HttpContext.AuthenticateAsync(AuthDefaults.AppCookieScheme);
        if (!result.Succeeded)
        {
            var returnUrl = Request.PathBase + Request.Path + QueryString.Create(
                Request.HasFormContentType ? [.. Request.Form] : [.. Request.Query]);
            return Challenge(
                new AuthenticationProperties { RedirectUri = "/login?returnUrl=" + Uri.EscapeDataString(returnUrl) },
                AuthDefaults.AppCookieScheme);
        }

        var user = await userManager.GetUserAsync(result.Principal!)
            ?? throw new InvalidOperationException("User not found.");

        if (!user.IsActive)
        {
            return Forbid(
                new AuthenticationProperties { RedirectUri = $"/login?error={AccountDisabled}" },
                AuthDefaults.AppCookieScheme);
        }

        // Verify the client is enabled
        var clientInfo = await clientService.GetByClientIdAsync(request.ClientId!);
        if (clientInfo is not { IsEnabled: true })
        {
            return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        var application = await applicationManager.FindByClientIdAsync(request.ClientId!)
            ?? throw new InvalidOperationException("Application not found.");

        var subject = await userManager.GetUserIdAsync(user);

        // Retrieve or create a permanent authorization for this subject+client
        var authorizations = await authorizationManager.FindAsync(
            subject: subject,
            client: await applicationManager.GetIdAsync(application) ?? string.Empty,
            status: OpenIddictConstants.Statuses.Valid,
            type: OpenIddictConstants.AuthorizationTypes.Permanent,
            scopes: request.GetScopes()).ToListAsync();

        var identity = await BuildUserIdentityAsync(user, request.GetScopes());

        var authorization = authorizations.LastOrDefault();
        authorization ??= await authorizationManager.CreateAsync(
            identity: identity,
            subject: subject,
            client: await applicationManager.GetIdAsync(application) ?? string.Empty,
            type: OpenIddictConstants.AuthorizationTypes.Permanent,
            scopes: identity.GetScopes());

        identity.SetAuthorizationId(await authorizationManager.GetIdAsync(authorization));
        identity.SetDestinations(GetDestinations);

        return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    // ── Token endpoint ────────────────────────────────────────────────────────

    [HttpPost("~/connect/token")]
    [IgnoreAntiforgeryToken]
    [Produces("application/json")]
    public async Task<IActionResult> Exchange()
    {
        var request = HttpContext.GetOpenIddictServerRequest()
            ?? throw new InvalidOperationException("OpenIddict request cannot be retrieved.");

        if (request.IsAuthorizationCodeGrantType() || request.IsRefreshTokenGrantType() || request.IsDeviceCodeGrantType())
        {
            return await ExchangeCodeOrRefreshOrDeviceAsync(request);
        }

        if (request.IsPasswordGrantType())
        {
            return await ExchangePasswordAsync(request);
        }

        if (request.IsClientCredentialsGrantType())
        {
            return await ExchangeClientCredentialsAsync(request);
        }

        throw new InvalidOperationException("Unsupported grant type.");
    }

    private async Task<IActionResult> ExchangeCodeOrRefreshOrDeviceAsync(OpenIddictRequest request)
    {
        var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        var userId = result.Principal?.GetClaim(OpenIddictConstants.Claims.Subject);

        if (userId is null)
        {
            return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user is null || !user.IsActive || await userManager.IsLockedOutAsync(user))
        {
            return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        var identity = await BuildUserIdentityAsync(user, result.Principal!.GetScopes());
        identity.SetAuthorizationId(result.Principal!.GetAuthorizationId());
        identity.SetDestinations(GetDestinations);

        return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    private async Task<IActionResult> ExchangePasswordAsync(OpenIddictRequest request)
    {
        var user = await userManager.FindByNameAsync(request.Username!)
                ?? await userManager.FindByEmailAsync(request.Username!);

        if (user is null || !await userManager.CheckPasswordAsync(user, request.Password!))
        {
            return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        if (!user.IsActive || await userManager.IsLockedOutAsync(user))
        {
            return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        var identity = await BuildUserIdentityAsync(user, request.GetScopes());
        identity.SetDestinations(GetDestinations);

        return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    private async Task<IActionResult> ExchangeClientCredentialsAsync(OpenIddictRequest request)
    {
        var clientInfo = await clientService.GetByClientIdAsync(request.ClientId!);
        if (clientInfo is not { IsEnabled: true })
        {
            return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        // Validate against our custom secret table for clients that have secrets configured
        var secrets = await clientService.GetSecretsAsync(clientInfo.Id!);
        if (secrets.Count > 0)
        {
            if (string.IsNullOrEmpty(request.ClientSecret) ||
                !await clientService.ValidateSecretAsync(clientInfo.Id!, request.ClientSecret))
            {
                return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }
        }

        var identity = new ClaimsIdentity(
            authenticationType: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            nameType: OpenIddictConstants.Claims.Name,
            roleType: OpenIddictConstants.Claims.Role);

        identity.SetClaim(OpenIddictConstants.Claims.Subject, clientInfo.ClientId);
        identity.SetClaim(OpenIddictConstants.Claims.Name, clientInfo.DisplayName);
        identity.SetScopes(request.GetScopes());
        identity.SetDestinations(_ => [Destinations.AccessToken]);

        return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    // ── Userinfo endpoint ─────────────────────────────────────────────────────

    [Authorize(AuthenticationSchemes = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)]
    [HttpGet("~/connect/userinfo")]
    [HttpPost("~/connect/userinfo")]
    public async Task<IActionResult> UserInfo()
    {
        var userId = User.GetClaim(OpenIddictConstants.Claims.Subject);
        if (userId is null)
        {
            return Challenge(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return Challenge(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        var claims = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            [OpenIddictConstants.Claims.Subject] = userId
        };

        if (User.HasScope(Scopes.Email))
        {
            claims[OpenIddictConstants.Claims.Email] = user.Email ?? string.Empty;
            claims[OpenIddictConstants.Claims.EmailVerified] = await userManager.IsEmailConfirmedAsync(user);
        }

        if (User.HasScope(Scopes.Profile))
        {
            claims[OpenIddictConstants.Claims.Name] = user.DisplayName ?? user.UserName ?? string.Empty;
            if (user.FirstName is not null)
            {
                claims[OpenIddictConstants.Claims.GivenName] = user.FirstName;
            }

            if (user.LastName is not null)
            {
                claims[OpenIddictConstants.Claims.FamilyName] = user.LastName;
            }
        }

        if (User.HasScope(Scopes.Roles))
        {
            claims[OpenIddictConstants.Claims.Role] = await userManager.GetRolesAsync(user);
        }

        return Ok(claims);
    }

    // ── End session endpoint ──────────────────────────────────────────────────

    [HttpGet("~/connect/endsession")]
    [HttpPost("~/connect/endsession")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> EndSession()
    {
        await HttpContext.SignOutAsync(AuthDefaults.AppCookieScheme);
        return SignOut(
            new AuthenticationProperties { RedirectUri = "/" },
            OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            AuthDefaults.AppCookieScheme);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<ClaimsIdentity> BuildUserIdentityAsync(ApplicationUser user, IEnumerable<string> scopes)
    {
        var identity = new ClaimsIdentity(
            authenticationType: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            nameType: OpenIddictConstants.Claims.Name,
            roleType: OpenIddictConstants.Claims.Role);

        identity
            .SetClaim(OpenIddictConstants.Claims.Subject, await userManager.GetUserIdAsync(user))
            .SetClaim(OpenIddictConstants.Claims.Email, await userManager.GetEmailAsync(user))
            .SetClaim(OpenIddictConstants.Claims.EmailVerified,
                (await userManager.IsEmailConfirmedAsync(user)).ToString().ToLowerInvariant())
            .SetClaim(OpenIddictConstants.Claims.Name, user.DisplayName ?? user.UserName)
            .SetClaim(OpenIddictConstants.Claims.GivenName, user.FirstName)
            .SetClaim(OpenIddictConstants.Claims.FamilyName, user.LastName)
            .SetClaims(OpenIddictConstants.Claims.Role, [.. await userManager.GetRolesAsync(user)]);

        identity.SetScopes(scopes);

        return identity;
    }

    private static IEnumerable<string> GetDestinations(Claim claim)
    {
        var identity = (ClaimsIdentity)claim.Subject!;

        return claim.Type switch
        {
            OpenIddictConstants.Claims.Subject =>
                [Destinations.AccessToken, Destinations.IdentityToken],

            OpenIddictConstants.Claims.Email or OpenIddictConstants.Claims.EmailVerified =>
                identity.GetScopes().Contains(Scopes.Email)
                    ? [Destinations.AccessToken, Destinations.IdentityToken]
                    : [],

            OpenIddictConstants.Claims.Name or
            OpenIddictConstants.Claims.GivenName or
            OpenIddictConstants.Claims.FamilyName =>
                identity.GetScopes().Contains(Scopes.Profile)
                    ? [Destinations.AccessToken, Destinations.IdentityToken]
                    : [],

            OpenIddictConstants.Claims.Role =>
                identity.GetScopes().Contains(Scopes.Roles)
                    ? [Destinations.AccessToken]
                    : [],

            _ => [Destinations.AccessToken]
        };
    }
}
