namespace Mothenticate.Data.Entities;

public class AuditLog
{
    public long Id { get; set; }
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    public string? ActorUserId { get; set; }
    public required string ResourceType { get; set; }
    public required string ResourceId { get; set; }
    public AuditAction Action { get; set; }
}
