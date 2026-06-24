namespace Mothenticate.Data.Entities;

public class AppLauncherAccess
{
    public int Id { get; set; }
    public int AppLauncherId { get; set; }
    public string? UserId { get; set; }
    public int? GroupId { get; set; }

    public AppLauncher AppLauncher { get; set; } = null!;
    public ApplicationUser? User { get; set; }
    public Group? Group { get; set; }
}
