namespace Mothenticate.Data.Entities;

public class AppSettings
{
    // Singleton row — always Id = 1
    public int Id { get; set; } = 1;

    // User management
    public bool UseEmailAsUsername { get; set; } = false;
    public bool AllowEmailLogin { get; set; } = false;
    public bool RegistrationEnabled { get; set; } = true;

    // Localization
    public string DefaultLanguage { get; set; } = "en";

    // Branding
    public string? BrandingTitle { get; set; }
    public string? BrandingSubtitle { get; set; }


}
