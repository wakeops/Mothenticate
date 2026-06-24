namespace Mothenticate.IdentityProvider.Models;

public record ClientSecretInfo
{
    public int Id { get; init; }
    public string? Description { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? ExpiresAt { get; init; }
    public bool IsExpired => ExpiresAt.HasValue && ExpiresAt.Value < DateTimeOffset.UtcNow;
}
