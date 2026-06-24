using Microsoft.AspNetCore.Identity;
using Mothenticate.Data.Entities;

namespace Mothenticate.IdentityProvider.Services;

public class TwoFactorService(UserManager<ApplicationUser> userManager) : ITwoFactorService
{
    public async Task<bool> IsEnabledAsync(string userId)
    {
        var user = await GetUserAsync(userId);
        return user.TwoFactorEnabled;
    }

    public async Task<string> GetOrCreateKeyAsync(string userId)
    {
        var user = await GetUserAsync(userId);
        var key = await userManager.GetAuthenticatorKeyAsync(user);
        if (string.IsNullOrEmpty(key))
        {
            await userManager.ResetAuthenticatorKeyAsync(user);
            key = await userManager.GetAuthenticatorKeyAsync(user);
        }
        return key ?? throw new InvalidOperationException("Failed to get authenticator key.");
    }

    public async Task<bool> VerifyTotpAsync(string userId, string code)
    {
        var user = await GetUserAsync(userId);
        var stripped = code.Replace(" ", string.Empty).Replace("-", string.Empty);
        return await userManager.VerifyTwoFactorTokenAsync(user, TokenOptions.DefaultAuthenticatorProvider, stripped);
    }

    public async Task EnableAsync(string userId)
    {
        var user = await GetUserAsync(userId);
        await userManager.SetTwoFactorEnabledAsync(user, true);
    }

    public async Task DisableAsync(string userId)
    {
        var user = await GetUserAsync(userId);
        await userManager.SetTwoFactorEnabledAsync(user, false);
    }

    public async Task<IReadOnlyList<string>> GenerateRecoveryCodesAsync(string userId, int count = 10)
    {
        var user = await GetUserAsync(userId);
        var codes = await userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, count);
        return codes?.ToList() ?? [];
    }

    public async Task<bool> RedeemRecoveryCodeAsync(string userId, string code)
    {
        var user = await GetUserAsync(userId);
        var result = await userManager.RedeemTwoFactorRecoveryCodeAsync(user, code.Trim());
        return result.Succeeded;
    }

    public async Task<int> CountRecoveryCodesAsync(string userId)
    {
        var user = await GetUserAsync(userId);
        return await userManager.CountRecoveryCodesAsync(user);
    }

    public string BuildOtpAuthUri(string key, string userIdentifier, string issuer)
    {
        var encodedIssuer = Uri.EscapeDataString(issuer);
        var encodedUser = Uri.EscapeDataString(userIdentifier);
        return $"otpauth://totp/{encodedIssuer}:{encodedUser}?secret={key}&issuer={encodedIssuer}&algorithm=SHA1&digits=6&period=30";
    }

    private async Task<ApplicationUser> GetUserAsync(string userId)
        => await userManager.FindByIdAsync(userId)
           ?? throw new InvalidOperationException($"User {userId} not found.");
}
