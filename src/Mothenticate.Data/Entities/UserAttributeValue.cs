namespace Mothenticate.Data.Entities;

public class UserAttributeValue
{
    public int Id { get; set; }
    public required string UserId { get; set; }
    public int UserAttributeId { get; set; }
    public string? Value { get; set; }
    public int Ordinal { get; set; }

    public ApplicationUser User { get; set; } = null!;
    public UserAttribute UserAttribute { get; set; } = null!;
}
