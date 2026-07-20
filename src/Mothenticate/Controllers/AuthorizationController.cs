using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Mothenticate.Data.Entities;
using Mothenticate.Domain.Config;
using Mothenticate.IdentityProvider.Services;
using Mothenticate.IdentityProvider.Services.ScopeMappers;
using Mothenticate.IdentityProvider.Services.ScopeMappers.Abstract;
using Mothenticate.IdentityProvider.Services.ScopeMappers.Handlers;
using Mothenticate.UserManagement.Services;
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
    IUserAttributeService userAttributeService,
    IClientScopeService clientScopeService,
    IScopeMapperResolver scopeMapperResolver,
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

        var scopes = User.GetScopes().ToList();

        if (scopes.Contains(Scopes.Roles))
        {
            claims[OpenIddictConstants.Claims.Role] = await userManager.GetRolesAsync(user);
        }

        var mapperRows = await clientScopeService.GetMappersByScopeNamesAsync(scopes);
        if (mapperRows.Count > 0)
        {
            var userAttributes = await userAttributeService.GetAllWithUserValuesAsync(userId);
            var identity = User.Identity as ClaimsIdentity ?? new ClaimsIdentity();
            var userInfo = new Dictionary<string, string>();

            foreach (var mapperRow in mapperRows)
            {
                if (scopeMapperResolver.Resolve(mapperRow.MapperType) is not IUserInfoMapper userInfoMapper)
                {
                    continue;
                }

                var config = DeserializeConfig(mapperRow.Config);
                await UserInfoMapperHandler.HandleAsync(userInfoMapper, config, identity, user, userAttributes, userInfo, HttpContext.RequestAborted);
            }

            foreach (var (key, value) in userInfo)
            {
                claims[key] = value;
            }
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

        var scopeList = scopes.ToList();
        var userId = await userManager.GetUserIdAsync(user);

        AddClaim(identity, OpenIddictConstants.Claims.Subject, userId, Destinations.AccessToken, Destinations.IdentityToken);

        if (scopeList.Contains(Scopes.Roles))
        {
            foreach (var role in await userManager.GetRolesAsync(user))
            {
                AddClaim(identity, OpenIddictConstants.Claims.Role, role, Destinations.AccessToken);
            }
        }

        await ApplyScopeMapperClaimsAsync(identity, user, scopeList);

        identity.SetScopes(scopeList);

        return identity;
    }

    private async Task ApplyScopeMapperClaimsAsync(ClaimsIdentity identity, ApplicationUser user, IReadOnlyList<string> scopes)
    {
        var mapperRows = await clientScopeService.GetMappersByScopeNamesAsync(scopes);
        if (mapperRows.Count == 0)
        {
            return;
        }

        var userAttributes = await userAttributeService.GetAllWithUserValuesAsync(user.Id);

        foreach (var mapperRow in mapperRows)
        {
            if (scopeMapperResolver.Resolve(mapperRow.MapperType) is not ITokenMapper tokenMapper)
            {
                continue;
            }

            var config = DeserializeConfig(mapperRow.Config);
            await TokenMapperHandler.HandleAsync(tokenMapper, config, identity, user, userAttributes, HttpContext.RequestAborted);
        }
    }

    private static Dictionary<string, string> DeserializeConfig(string config)
        => JsonSerializer.Deserialize<Dictionary<string, string>>(config) ?? [];

    private static void AddClaim(ClaimsIdentity identity, string type, string value, params string[] destinations)
    {
        var claim = new Claim(type, value);
        claim.SetDestinations(destinations);
        identity.AddClaim(claim);
    }
}
