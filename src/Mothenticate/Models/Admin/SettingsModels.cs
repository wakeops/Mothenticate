namespace Mothenticate.Models.Admin;

public record SettingsResponse
{
    public bool UseEmailAsUsername { get; init; }
    public bool AvatarsEnabled { get; init; }
    public bool RegistrationEnabled { get; init; }
    public string? BrandingTitle { get; init; }
    public string? BrandingSubtitle { get; init; }
    public bool GoogleSsoEnabled { get; init; }
    public string? GoogleClientId { get; init; }
    public bool GitHubSsoEnabled { get; init; }
    public string? GitHubClientId { get; init; }
}

public record UpdateSettingsRequest
{
    public bool UseEmailAsUsername { get; init; }
    public bool AvatarsEnabled { get; init; }
    public bool RegistrationEnabled { get; init; }
    public string? BrandingTitle { get; init; }
    public string? BrandingSubtitle { get; init; }
    public bool GoogleSsoEnabled { get; init; }
    public string? GoogleClientId { get; init; }
    public string? GoogleClientSecret { get; init; }
    public bool GitHubSsoEnabled { get; init; }
    public string? GitHubClientId { get; init; }
    public string? GitHubClientSecret { get; init; }
}
