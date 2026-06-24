namespace Mothenticate.Domain.Config;

public static class AuthDefaults
{
    public const string AppCookieScheme = "mothenticate-cookie";

    public const string AppBearerAudience = "default-mothenticate-auth";

    public const string AppAdminUserPolicy = "IsAppAdminUser";
    public const string AppAdminUserRole = "mothenticate-admin";

    public const string PreferredUsernameClaimType = "preferred_username";
}

