namespace Mothenticate.IdentityProvider.Sso;

public static class SsoDefaults
{
    public const string GoogleScheme = "Google";
    public const string GitHubScheme = "GitHub";
    public const string ExternalCookieScheme = "mothenticate-external";
    public const string PreAuthCookieScheme = "mothenticate-2fa-preauth";

    /// <summary>
    /// Claim type used to stash the external provider's raw userinfo JSON on the external cookie
    /// principal, so AttributeImporter mappers can read arbitrary fields out of it during Finalize —
    /// well past the point where the original HTTP response is available.
    /// </summary>
    public const string RawProfileClaimType = "mothenticate:raw_profile";
}
