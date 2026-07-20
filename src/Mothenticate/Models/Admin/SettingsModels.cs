namespace Mothenticate.Models.Admin;

public record SettingsResponse
{
    public bool UseEmailAsUsername { get; init; }
    public bool RegistrationEnabled { get; init; }
    public string? BrandingTitle { get; init; }
    public string? BrandingSubtitle { get; init; }
}

public record UpdateSettingsRequest
{
    public bool UseEmailAsUsername { get; init; }
    public bool RegistrationEnabled { get; init; }
    public string? BrandingTitle { get; init; }
    public string? BrandingSubtitle { get; init; }
}
