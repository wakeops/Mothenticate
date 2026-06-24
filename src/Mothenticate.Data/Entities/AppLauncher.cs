namespace Mothenticate.Data.Entities;

public class AppLauncher
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public string? Description { get; set; }
    public string? IconUrl { get; set; }
    public string? LaunchUrl { get; set; }
    public string? LinkedClientId { get; set; }
    public bool IsEnabled { get; set; } = true;
    public bool IsPublic { get; set; } = true;

    public ICollection<AppLauncherAccess> AccessEntries { get; set; } = [];
}
