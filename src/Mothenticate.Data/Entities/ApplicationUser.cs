using Microsoft.AspNetCore.Identity;

namespace Mothenticate.Data.Entities;

public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? DisplayName { get; set; }
    public byte[]? AvatarData { get; set; }
    public string? AvatarContentType { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<UserGroup> UserGroups { get; set; } = [];
    public ICollection<UserPropertyValue> PropertyValues { get; set; } = [];
    public ICollection<UserSession> Sessions { get; set; } = [];
}
