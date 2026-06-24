using Microsoft.AspNetCore.Identity;
using Mothenticate.Data.Entities;

namespace Mothenticate.IdentityProvider.Services;

public class SsoService(UserManager<ApplicationUser> userManager) : ISsoService
{
    public async Task<IReadOnlyList<UserLoginInfo>> GetLoginsAsync(string userId)
    {
        var user = await userManager.FindByIdAsync(userId)
            ?? throw new InvalidOperationException($"User {userId} not found.");
        return (await userManager.GetLoginsAsync(user)).ToList();
    }

    public async Task<IdentityResult> LinkLoginAsync(string userId, string provider, string providerKey, string displayName)
    {
        var user = await userManager.FindByIdAsync(userId)
            ?? throw new InvalidOperationException($"User {userId} not found.");

        var existing = await userManager.FindByLoginAsync(provider, providerKey);
        if (existing is not null && existing.Id != userId)
        {
            return IdentityResult.Failed(new IdentityError { Description = "This account is already linked to another user." });
        }

        return await userManager.AddLoginAsync(user, new UserLoginInfo(provider, providerKey, displayName));
    }

    public async Task<IdentityResult> UnlinkLoginAsync(string userId, string provider, string providerKey)
    {
        var user = await userManager.FindByIdAsync(userId)
            ?? throw new InvalidOperationException($"User {userId} not found.");
        return await userManager.RemoveLoginAsync(user, provider, providerKey);
    }
}
