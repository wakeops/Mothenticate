using Microsoft.AspNetCore.Identity;

namespace Mothenticate.Data.Entities;

public class ApplicationUser : IdentityUser
{
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<UserGroup> UserGroups { get; set; } = [];
    public ICollection<UserAttributeValue> AttributeValues { get; set; } = [];
    public ICollection<UserSession> Sessions { get; set; } = [];
}
