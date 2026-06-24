using System.ComponentModel.DataAnnotations;

namespace Mothenticate.Models.Admin;

public record RoleResponse
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
}

public record CreateRoleRequest
{
    [Required] public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
}

public record UpdateRoleRequest
{
    [Required] public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
}
