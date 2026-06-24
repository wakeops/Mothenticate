using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Mothenticate.Data.Entities;
using Mothenticate.Data.Services;
using Mothenticate.Domain.Config;
using Mothenticate.IdentityProvider.Services;
using Mothenticate.IdentityProvider.Sso;
using Mothenticate.UserManagement.Services;

namespace Mothenticate.Controllers;

using static AuthErrorCodes;

[Route("account")]
public class AccountController(
    UserManager<ApplicationUser> userManager,
    IAppSettingsService settingsService,
    ITwoFactorService twoFactorService,
    ISeedService seedService,
    SetupState setupState) : Controller
{
    [HttpPost("login")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Login(
        [FromForm] string username,
        [FromForm] string password,
        [FromForm] bool rememberMe = false,
        [FromForm] string? returnUrl = null)
    {
        returnUrl = string.IsNullOrEmpty(returnUrl) ? "/portal" : returnUrl;

        var settings = await settingsService.GetAsync();

        var user = settings.UseEmailAsUsername
            ? await userManager.FindByEmailAsync(username)
            : await userManager.FindByNameAsync(username)
              ?? await userManager.FindByEmailAsync(username);

        if (user is null)
        {
            return Redirect($"/login?error={InvalidCredentials}");
        }

        if (await userManager.IsLockedOutAsync(user))
        {
            return Redirect($"/login?error={AccountLocked}");
        }

        if (!user.IsActive)
        {
            return Redirect($"/login?error={AccountInactive}");
        }

        if (!await userManager.CheckPasswordAsync(user, password))
        {
            await userManager.AccessFailedAsync(user);
            return Redirect($"/login?error={InvalidCredentials}");
        }

        await userManager.ResetAccessFailedCountAsync(user);

        // 2FA check
        if (user.TwoFactorEnabled)
        {
            var preAuthPrincipal = new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim("remember_me", rememberMe ? "true" : "false"),
                new Claim("return_url", returnUrl)
            ], SsoDefaults.PreAuthCookieScheme));

            await HttpContext.SignInAsync(SsoDefaults.PreAuthCookieScheme, preAuthPrincipal,
                new AuthenticationProperties { ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(5) });

            return Redirect("/login/2fa");
        }

        await SignInFullAsync(user, rememberMe);

        return Url.IsLocalUrl(returnUrl) ? Redirect(returnUrl) : Redirect("/portal");
    }

    [HttpPost("verify2fa")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> VerifyTwoFactor(
        [FromForm] string code,
        [FromForm] bool isRecoveryCode = false)
    {
        var preAuth = await HttpContext.AuthenticateAsync(SsoDefaults.PreAuthCookieScheme);
        if (!preAuth.Succeeded)
        {
            return Redirect($"/login?error={SessionExpired}");
        }

        var userId = preAuth.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
        var returnUrl = preAuth.Principal?.FindFirstValue("return_url") ?? "/portal";
        var rememberMe = preAuth.Principal?.FindFirstValue("remember_me") == "true";

        if (userId is null)
        {
            return Redirect($"/login?error={SessionExpired}");
        }

        var valid = isRecoveryCode
            ? await twoFactorService.RedeemRecoveryCodeAsync(userId, code)
            : await twoFactorService.VerifyTotpAsync(userId, code);

        if (!valid)
        {
            return Redirect($"/login/2fa?error={InvalidCredentials}");
        }

        await HttpContext.SignOutAsync(SsoDefaults.PreAuthCookieScheme);

        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return Redirect("/login");
        }

        await SignInFullAsync(user, rememberMe);

        return Url.IsLocalUrl(returnUrl) ? Redirect(returnUrl) : Redirect("/portal");
    }

    [HttpPost("register")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Register(
        [FromForm] string username,
        [FromForm] string email,
        [FromForm] string password,
        [FromForm] string? firstName,
        [FromForm] string? lastName)
    {
        var settings = await settingsService.GetAsync();

        if (!settings.RegistrationEnabled)
        {
            return Redirect($"/register?error={RegistrationDisabled}");
        }

        var effectiveUsername = settings.UseEmailAsUsername ? email : username;

        var result = await userManager.CreateAsync(new ApplicationUser
        {
            UserName = effectiveUsername,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            DisplayName = string.IsNullOrWhiteSpace(firstName) ? effectiveUsername : firstName
        }, password);

        if (!result.Succeeded)
        {
            var msg = Uri.EscapeDataString(result.Errors.FirstOrDefault()?.Description ?? "Registration failed.");
            return Redirect($"/register?error={msg}");
        }

        var user = await userManager.FindByNameAsync(effectiveUsername)
                   ?? await userManager.FindByEmailAsync(email);

        if (user is null)
        {
            return Redirect("/login");
        }

        await SignInFullAsync(user, false);
        return Redirect("/portal");
    }

    [HttpPost("setup")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Setup(
        [FromForm] string username,
        [FromForm] string email,
        [FromForm] string password)
    {
        if (!await seedService.IsFirstRunAsync())
            return Redirect("/login");

        var user = new ApplicationUser
        {
            UserName = username,
            Email = email,
            EmailConfirmed = true,
            IsActive = true,
            DisplayName = username
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            var msg = Uri.EscapeDataString(result.Errors.FirstOrDefault()?.Description ?? "Failed to create account.");
            return Redirect($"/setup?error={msg}");
        }

        await userManager.AddToRoleAsync(user, AuthDefaults.AppAdminUserRole);
        setupState.MarkConfigured();

        await SignInFullAsync(user, false);
        return Redirect("/admin");
    }

    [HttpGet("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(AuthDefaults.AppCookieScheme);
        return Redirect("/login");
    }

    private async Task SignInFullAsync(ApplicationUser user, bool rememberMe)
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

        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, AuthDefaults.AppCookieScheme));

        await HttpContext.SignInAsync(AuthDefaults.AppCookieScheme, principal, new AuthenticationProperties
        {
            IsPersistent = rememberMe,
            ExpiresUtc = rememberMe ? DateTimeOffset.UtcNow.AddDays(30) : null
        });
    }
}
