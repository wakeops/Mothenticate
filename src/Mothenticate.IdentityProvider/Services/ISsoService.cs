using Microsoft.AspNetCore.Identity;

namespace Mothenticate.IdentityProvider.Services;

public interface ISsoService
{
    Task<IReadOnlyList<UserLoginInfo>> GetLoginsAsync(string userId);
    Task<IdentityResult> LinkLoginAsync(string userId, string provider, string providerKey, string displayName);
    Task<IdentityResult> UnlinkLoginAsync(string userId, string provider, string providerKey);
}
