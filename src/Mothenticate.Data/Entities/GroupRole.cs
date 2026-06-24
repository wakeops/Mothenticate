namespace Mothenticate.Data.Entities;

public class GroupRole
{
    public int GroupId { get; set; }
    public int RoleId { get; set; }

    public Group Group { get; set; } = null!;
    public AppRole Role { get; set; } = null!;
}
