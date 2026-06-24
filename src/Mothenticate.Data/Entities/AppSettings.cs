namespace Mothenticate.Data.Entities;

public class AppSettings
{
    // Singleton row — always Id = 1
    public int Id { get; set; } = 1;

    // User management
    public bool UseEmailAsUsername { get; set; } = false;
    public bool AvatarsEnabled { get; set; } = false;
    public bool RegistrationEnabled { get; set; } = true;

    // Branding
    public string? BrandingTitle { get; set; }
    public string? BrandingSubtitle { get; set; }

    // SSO — Google
    public bool GoogleSsoEnabled { get; set; } = false;
    public string? GoogleClientId { get; set; }
    public string? GoogleClientSecret { get; set; }

    // SSO — GitHub
    public bool GitHubSsoEnabled { get; set; } = false;
    public string? GitHubClientId { get; set; }
    public string? GitHubClientSecret { get; set; }
}
