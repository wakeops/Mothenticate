namespace Mothenticate.Data.Entities;

public class Group
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }

    public ICollection<UserGroup> UserGroups { get; set; } = [];
    public ICollection<GroupRole> GroupRoles { get; set; } = [];
}
