using System.ComponentModel.DataAnnotations;
using Mothenticate.Data.Entities;

namespace Mothenticate.Models.Admin;

public record UserPropertyResponse
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public PropertyType Type { get; init; }
    public bool IsRequired { get; init; }
    public bool IsHidden { get; init; }
    public bool IsReadOnly { get; init; }
    public int SortOrder { get; init; }
}

public record CreateUserPropertyRequest
{
    [Required] public string Name { get; init; } = string.Empty;
    [Required] public string DisplayName { get; init; } = string.Empty;
    public PropertyType Type { get; init; } = PropertyType.Text;
    public bool IsRequired { get; init; }
    public bool IsHidden { get; init; }
    public bool IsReadOnly { get; init; }
}

public record UpdateUserPropertyRequest
{
    [Required] public string DisplayName { get; init; } = string.Empty;
    public PropertyType Type { get; init; } = PropertyType.Text;
    public bool IsRequired { get; init; }
    public bool IsHidden { get; init; }
    public bool IsReadOnly { get; init; }
}
