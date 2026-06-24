namespace Mothenticate.Data.Entities;

public class UserPropertyValue
{
    public int Id { get; set; }
    public required string UserId { get; set; }
    public int PropertyId { get; set; }
    public string? Value { get; set; }

    public ApplicationUser User { get; set; } = null!;
    public UserProperty Property { get; set; } = null!;
}
