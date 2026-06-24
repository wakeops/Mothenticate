using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Mothenticate.Data.Entities;
using Mothenticate.Domain.Config;
using Mothenticate.IdentityProvider.Services;
using Mothenticate.IdentityProvider.Sso;
using Mothenticate.UserManagement.Services;

namespace Mothenticate.Controllers;

using static AuthErrorCodes;

[Route("connect/external")]
public class ExternalAuthController(
    UserManager<ApplicationUser> userManager,
    IAppSettingsService settingsService,
    ISsoService ssoService) : Controller
{
    [HttpGet("{provider}")]
    public async Task<IActionResult> Challenge(
        string provider,
        [FromQuery] string? returnUrl = null,
        [FromQuery] string? linkUserId = null)
    {
        var settings = await settingsService.GetAsync();
        var scheme = NormalizeScheme(provider);

        var enabled = scheme switch
        {
            SsoDefaults.GoogleScheme => settings.GoogleSsoEnabled,
            SsoDefaults.GitHubScheme => settings.GitHubSsoEnabled,
            _                        => false
        };

        if (!enabled)
        {
            return Redirect("/login");
        }

        var redirectUri = Url.Action("Finalize", "ExternalAuth", new { provider = scheme, returnUrl, linkUserId });
        var props = new AuthenticationProperties { RedirectUri = redirectUri };
        return Challenge(props, scheme);
    }

    [HttpGet("{provider}/finalize")]
    public async Task<IActionResult> Finalize(
        string provider,
        [FromQuery] string? returnUrl = null,
        [FromQuery] string? linkUserId = null)
    {
        var result = await HttpContext.AuthenticateAsync(SsoDefaults.ExternalCookieScheme);
        if (!result.Succeeded)
        {
            return Redirect($"/login?error={SsoFailed}");
        }

        await HttpContext.SignOutAsync(SsoDefaults.ExternalCookieScheme);

        var scheme = NormalizeScheme(provider);
        var externalId = result.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = result.Principal?.FindFirstValue(ClaimTypes.Email);
        var displayName = result.Principal?.FindFirstValue(ClaimTypes.Name) ?? email;

        if (externalId is null)
        {
            return Redirect($"/login?error={SsoFailed}");
        }

        // Link flow — user is already signed in and linking a new provider
        if (!string.IsNullOrEmpty(linkUserId))
        {
            var linkResult = await ssoService.LinkLoginAsync(linkUserId, scheme, externalId, displayName ?? scheme);
            if (!linkResult.Succeeded)
            {
                return Redirect($"/portal/profile?error={LinkFailed}");
            }

            await RefreshSignIn(linkUserId);
            return Redirect("/portal/profile");
        }

        // Sign-in flow — find or create user
        var user = await userManager.FindByLoginAsync(scheme, externalId);

        if (user is null && email is not null)
        {
            user = await userManager.FindByEmailAsync(email);
        }

        if (user is null)
        {
            var settings = await settingsService.GetAsync();
            if (!settings.RegistrationEnabled)
            {
                return Redirect($"/login?error={RegistrationDisabledSso}");
            }

            var username = email ?? externalId;
            user = new ApplicationUser
            {
                UserName = username,
                Email = email,
                DisplayName = displayName ?? username,
                IsActive = true
            };

            var createResult = await userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                return Redirect($"/login?error={SsoFailed}");
            }
        }

        // Ensure login is linked
        var existing = await userManager.GetLoginsAsync(user);
        if (!existing.Any(l => l.LoginProvider == scheme && l.ProviderKey == externalId))
        {
            await userManager.AddLoginAsync(user, new UserLoginInfo(scheme, externalId, displayName ?? scheme));
        }

        if (!user.IsActive)
        {
            return Redirect($"/login?error={AccountInactive}");
        }

        await SignInAsync(user);

        return Url.IsLocalUrl(returnUrl) ? Redirect(returnUrl) : Redirect("/portal");
    }

    private async Task SignInAsync(ApplicationUser user)
    {
        var roles = await userManager.GetRolesAsync(user);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName ?? user.Email ?? string.Empty),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(AuthDefaults.PreferredUsernameClaimType, user.DisplayName ?? user.UserName ?? user.Email ?? string.Empty),
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        await HttpContext.SignInAsync(AuthDefaults.AppCookieScheme,
            new ClaimsPrincipal(new ClaimsIdentity(claims, AuthDefaults.AppCookieScheme)));
    }

    private async Task RefreshSignIn(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is not null)
        {
            await SignInAsync(user);
        }
    }

    private static string NormalizeScheme(string provider) =>
        provider.ToLowerInvariant() switch
        {
            "google" => SsoDefaults.GoogleScheme,
            "github" => SsoDefaults.GitHubScheme,
            _        => provider
        };
}
