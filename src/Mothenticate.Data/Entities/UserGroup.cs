namespace Mothenticate.Data.Entities;

public class UserGroup
{
    public required string UserId { get; set; }
    public int GroupId { get; set; }

    public ApplicationUser User { get; set; } = null!;
    public Group Group { get; set; } = null!;
}
