namespace Mothenticate.Data.Entities;

public class AppRole
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }

    public ICollection<GroupRole> GroupRoles { get; set; } = [];
}
