using System.ComponentModel.DataAnnotations;

namespace Mothenticate.Models.Admin;

public record ClientResponse
{
    public string? Id { get; init; }
    public string? ClientId { get; init; }
    public string? DisplayName { get; init; }
    public string? ClientType { get; init; }
    public bool IsEnabled { get; init; }
    public IReadOnlyList<string> RedirectUris { get; init; } = [];
    public IReadOnlyList<string> PostLogoutRedirectUris { get; init; } = [];
    public IReadOnlyList<string> Permissions { get; init; } = [];
}

public record CreateClientRequest
{
    [Required] public string ClientId { get; init; } = string.Empty;
    [Required] public string DisplayName { get; init; } = string.Empty;
    public string ClientType { get; init; } = "public";
    public bool IsEnabled { get; init; } = true;
    public IReadOnlyList<string> RedirectUris { get; init; } = [];
    public IReadOnlyList<string> PostLogoutRedirectUris { get; init; } = [];
    public IReadOnlyList<string> Permissions { get; init; } = [];
}

public record UpdateClientRequest
{
    [Required] public string DisplayName { get; init; } = string.Empty;
    public string ClientType { get; init; } = "public";
    public IReadOnlyList<string> RedirectUris { get; init; } = [];
    public IReadOnlyList<string> PostLogoutRedirectUris { get; init; } = [];
    public IReadOnlyList<string> Permissions { get; init; } = [];
}

public record SetClientEnabledRequest
{
    public bool IsEnabled { get; init; }
}

public record AddSecretRequest
{
    public string? Description { get; init; }
    public DateTimeOffset? ExpiresAt { get; init; }
}

public record AddSecretResponse
{
    public int Id { get; init; }
    public string Secret { get; init; } = string.Empty;
    public string? Description { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? ExpiresAt { get; init; }
}

public record SecretResponse
{
    public int Id { get; init; }
    public string? Description { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? ExpiresAt { get; init; }
    public bool IsExpired { get; init; }
}
