using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Mothenticate.Data.Entities;
using Mothenticate.Domain.Config;
using Mothenticate.IdentityProvider.Services;
using Mothenticate.IdentityProvider.Services.IdentityProviderMappers;
using Mothenticate.IdentityProvider.Services.IdentityProviderMappers.Handlers;
using Mothenticate.IdentityProvider.Sso;
using Mothenticate.UserManagement.Services;
using IdpEntity = Mothenticate.Data.Entities.IdentityProvider;

namespace Mothenticate.Controllers;

using static AuthErrorCodes;

[Route("connect/external")]
public class ExternalAuthController(
    UserManager<ApplicationUser> userManager,
    IAppSettingsService settingsService,
    ISsoService ssoService,
    IIdentityProviderService identityProviderService,
    IIdentityProviderMapperResolver identityProviderMapperResolver,
    IUserAttributeService userAttributeService) : Controller
{
    [HttpGet("{provider}")]
    public async Task<IActionResult> Challenge(
        string provider,
        [FromQuery] string? returnUrl = null,
        [FromQuery] string? linkUserId = null)
    {
        var idp = await identityProviderService.GetProviderByAliasAsync(provider);
        if (idp is null || !idp.IsEnabled)
            return Redirect("/login");

        var redirectUri = Url.Action("Finalize", "ExternalAuth", new { provider, returnUrl, linkUserId });
        var props = new AuthenticationProperties { RedirectUri = redirectUri };
        return Challenge(props, provider);
    }

    [HttpGet("{provider}/finalize")]
    public async Task<IActionResult> Finalize(
        string provider,
        [FromQuery] string? returnUrl = null,
        [FromQuery] string? linkUserId = null)
    {
        var result = await HttpContext.AuthenticateAsync(SsoDefaults.ExternalCookieScheme);
        if (!result.Succeeded)
            return Redirect($"/login?error={SsoFailed}");

        await HttpContext.SignOutAsync(SsoDefaults.ExternalCookieScheme);

        var externalId = result.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = result.Principal?.FindFirstValue(ClaimTypes.Email);
        var displayName = result.Principal?.FindFirstValue(ClaimTypes.Name) ?? email;
        var rawProfileJson = result.Principal?.FindFirstValue(SsoDefaults.RawProfileClaimType);

        if (externalId is null)
            return Redirect($"/login?error={SsoFailed}");

        var idp = await identityProviderService.GetProviderByAliasAsync(provider, HttpContext.RequestAborted);

        // Link flow — user is already signed in and linking a new provider
        if (!string.IsNullOrEmpty(linkUserId))
        {
            var linkResult = await ssoService.LinkLoginAsync(linkUserId, provider, externalId, displayName ?? provider);
            if (!linkResult.Succeeded)
                return Redirect($"/portal/profile?error={LinkFailed}");

            if (idp is not null)
                await ApplyAttributeMappersAsync(idp, linkUserId, rawProfileJson);

            await RefreshSignIn(linkUserId);
            return Redirect("/portal/profile");
        }

        // Sign-in flow — find or create user
        var user = await userManager.FindByLoginAsync(provider, externalId);

        if (user is null && email is not null)
            user = await userManager.FindByEmailAsync(email);

        if (user is null)
        {
            var settings = await settingsService.GetAsync();
            if (!settings.RegistrationEnabled)
                return Redirect($"/login?error={RegistrationDisabledSso}");

            var username = email ?? externalId;
            user = new ApplicationUser
            {
                UserName = username,
                Email = email,
                IsActive = true
            };

            var createResult = await userManager.CreateAsync(user);
            if (!createResult.Succeeded)
                return Redirect($"/login?error={SsoFailed}");
        }

        // Ensure login is linked
        var existing = await userManager.GetLoginsAsync(user);
        if (!existing.Any(l => l.LoginProvider == provider && l.ProviderKey == externalId))
            await userManager.AddLoginAsync(user, new UserLoginInfo(provider, externalId, displayName ?? provider));

        if (!user.IsActive)
            return Redirect($"/login?error={AccountInactive}");

        if (idp is not null)
            await ApplyAttributeMappersAsync(idp, user.Id, rawProfileJson);

        await SignInAsync(user);
        return Url.IsLocalUrl(returnUrl) ? Redirect(returnUrl) : Redirect("/portal");
    }

    private async Task ApplyAttributeMappersAsync(IdpEntity idp, string userId, string? rawProfileJson)
    {
        if (rawProfileJson is null)
            return;

        using var doc = JsonDocument.Parse(rawProfileJson);
        await AttributeImportHandler.ApplyAsync(idp, userId, doc.RootElement, identityProviderMapperResolver, userAttributeService, HttpContext.RequestAborted);
    }

    private async Task SignInAsync(ApplicationUser user)
    {
        var roles = await userManager.GetRolesAsync(user);
        var attributeValues = await userAttributeService.GetUserValuesAsync(user.Id);
        var displayName = attributeValues
            .FirstOrDefault(v => v.UserAttribute.Name.Equals("displayName", StringComparison.OrdinalIgnoreCase))?.Value;

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName ?? user.Email ?? string.Empty),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(AuthDefaults.PreferredUsernameClaimType, displayName ?? user.UserName ?? user.Email ?? string.Empty),
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        await HttpContext.SignInAsync(AuthDefaults.AppCookieScheme,
            new ClaimsPrincipal(new ClaimsIdentity(claims, AuthDefaults.AppCookieScheme)));
    }

    private async Task RefreshSignIn(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is not null) await SignInAsync(user);
    }
}
