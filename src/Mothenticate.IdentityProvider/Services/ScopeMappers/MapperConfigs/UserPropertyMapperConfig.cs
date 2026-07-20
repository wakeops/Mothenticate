using System.ComponentModel.DataAnnotations;
using Mothenticate.Data.Entities;

namespace Mothenticate.IdentityProvider.Services.ScopeMappers.MapperConfigs;

public class UserPropertyMapperConfig
{
    [Required]
    [Display(Name = "Token Claim Name")]
    public string? TokenClaimName { get; set; }

    // A closed enum, not a free-text property name — this config drives reflection-free access to a
    // fixed, reviewed set of ApplicationUser fields. Admin-editable free text here would let a mapper
    // be pointed at any public property (PasswordHash, SecurityStamp, ...) and leak it via claims.
    [Required]
    [Display(Name = "User Property")]
    public UserPropertyField? UserProperty { get; set; }
}