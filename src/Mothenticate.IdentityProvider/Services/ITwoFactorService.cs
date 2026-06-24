using Mothenticate.Data.Entities;

namespace Mothenticate.IdentityProvider.Services;

public interface ITwoFactorService
{
    Task<bool> IsEnabledAsync(string userId);
    Task<string> GetOrCreateKeyAsync(string userId);
    Task<bool> VerifyTotpAsync(string userId, string code);
    Task EnableAsync(string userId);
    Task DisableAsync(string userId);
    Task<IReadOnlyList<string>> GenerateRecoveryCodesAsync(string userId, int count = 10);
    Task<bool> RedeemRecoveryCodeAsync(string userId, string code);
    Task<int> CountRecoveryCodesAsync(string userId);
    string BuildOtpAuthUri(string key, string userIdentifier, string issuer);
}
