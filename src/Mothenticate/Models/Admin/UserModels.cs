using System.ComponentModel.DataAnnotations;

namespace Mothenticate.Models.Admin;

public record UserSummary
{
    public string Id { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? DisplayName { get; init; }
    public bool IsActive { get; init; }
    public bool IsLockedOut { get; init; }
}

public record UserResponse
{
    public string Id { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? DisplayName { get; init; }
    public bool IsActive { get; init; }
    public bool IsLockedOut { get; init; }
    public bool HasAvatar { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

public record CreateUserRequest
{
    [Required] public string UserName { get; init; } = string.Empty;
    [Required, EmailAddress] public string Email { get; init; } = string.Empty;
    [Required, MinLength(8)] public string Password { get; init; } = string.Empty;
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? DisplayName { get; init; }
    public bool IsActive { get; init; } = true;
}

public record UpdateUserRequest
{
    [Required] public string UserName { get; init; } = string.Empty;
    [Required, EmailAddress] public string Email { get; init; } = string.Empty;
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? DisplayName { get; init; }
    public bool IsActive { get; init; } = true;
}

public record SetPasswordRequest
{
    [Required, MinLength(8)] public string NewPassword { get; init; } = string.Empty;
}

public record AssignRoleRequest
{
    [Required] public string RoleName { get; init; } = string.Empty;
}
