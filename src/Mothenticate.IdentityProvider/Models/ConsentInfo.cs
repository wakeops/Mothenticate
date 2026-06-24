namespace Mothenticate.IdentityProvider.Models;

public record ConsentInfo
{
    public string? Id { get; init; }
    public string? Subject { get; init; }
    public string? ApplicationId { get; init; }
    public string? ApplicationName { get; init; }
    public string? Status { get; init; }
    public IReadOnlyList<string> Scopes { get; init; } = [];
    public DateTimeOffset? CreationDate { get; init; }
}
