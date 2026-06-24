namespace Mothenticate;

internal static class AuthErrorCodes
{
    internal const string InvalidCredentials     = "invalid";
    internal const string AccountLocked          = "locked";
    internal const string AccountInactive        = "inactive";
    internal const string SessionExpired         = "expired";
    internal const string RegistrationDisabled   = "disabled";
    internal const string RegistrationDisabledSso = "registration_disabled";
    internal const string AccountDisabled        = "account_disabled";
    internal const string SsoFailed              = "sso_failed";
    internal const string LinkFailed             = "link_failed";
}
