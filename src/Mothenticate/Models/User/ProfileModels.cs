using System.ComponentModel.DataAnnotations;

namespace Mothenticate.Models.User;

public record ProfileResponse
{
    public string Id { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? DisplayName { get; init; }
    public bool HasAvatar { get; init; }
}

public record UpdateProfileRequest
{
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? DisplayName { get; init; }
}

public record ChangePasswordRequest
{
    [Required] public string CurrentPassword { get; init; } = string.Empty;
    [Required, MinLength(8)] public string NewPassword { get; init; } = string.Empty;
}

public record SessionResponse
{
    public Guid Id { get; init; }
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
    public string? DeviceLabel { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset LastActiveAt { get; init; }
}

public record ConsentResponse
{
    public string Id { get; init; } = string.Empty;
    public string? ApplicationName { get; init; }
    public IReadOnlyList<string> Scopes { get; init; } = [];
    public DateTimeOffset? CreationDate { get; init; }
}
