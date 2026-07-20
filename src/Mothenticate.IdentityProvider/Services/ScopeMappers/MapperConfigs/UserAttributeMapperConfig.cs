using System.ComponentModel.DataAnnotations;
using Mothenticate.IdentityProvider.Services.ScopeMappers.Abstract;

namespace Mothenticate.IdentityProvider.Services.ScopeMappers.MapperConfigs;

public class UserAttributeMapperConfig
{
    [Required]
    [Display(Name = "Token Claim Name")]
    public string? TokenClaimName { get; set; }

    [Required]
    [Display(Name = "User Attribute")]
    [UserAttributeReference]
    public int? UserAttributeId { get; set; }
}