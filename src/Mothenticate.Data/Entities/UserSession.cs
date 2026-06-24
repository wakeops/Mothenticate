namespace Mothenticate.Data.Entities;

public class UserSession
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string UserId { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? DeviceLabel { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset LastActiveAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? RevokedAt { get; set; }

    public ApplicationUser User { get; set; } = null!;
}
