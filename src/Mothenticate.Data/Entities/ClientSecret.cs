namespace Mothenticate.Data.Entities;

public class ClientSecret
{
    public int Id { get; set; }
    // References the OpenIddict application ID (string PK managed by OpenIddict)
    public required string ApplicationId { get; set; }
    public required string SecretHash { get; set; }
    public string? Description { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ExpiresAt { get; set; }
}
