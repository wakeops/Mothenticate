using System.ComponentModel.DataAnnotations;

namespace Mothenticate.Models.Admin;

public record GroupResponse
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
}

public record CreateGroupRequest
{
    [Required] public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
}

public record UpdateGroupRequest
{
    [Required] public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
}
