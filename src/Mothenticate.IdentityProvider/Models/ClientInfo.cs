namespace Mothenticate.IdentityProvider.Models;

public record ClientInfo
{
    public string? Id { get; init; }
    public string? ClientId { get; init; }
    public string? DisplayName { get; init; }
    public string? ClientType { get; init; }
    public IReadOnlyList<string> RedirectUris { get; init; } = [];
    public IReadOnlyList<string> PostLogoutRedirectUris { get; init; } = [];
    public IReadOnlyList<string> Permissions { get; init; } = [];
    public bool IsEnabled { get; init; } = true;
    public int? AccessTokenLifetime { get; init; }
    public int? IdentityTokenLifetime { get; init; }
    public int? RefreshTokenLifetime { get; init; }
}
