using System.ComponentModel.DataAnnotations;
using Mothenticate.IdentityProvider.Services.ScopeMappers.Abstract;

namespace Mothenticate.IdentityProvider.Services.IdentityProviderMappers.MapperConfigs;

public class AttributeImporterMapperConfig
{
    [Required]
    [Display(Name = "Provider JSON Field Path")]
    public string? ProviderFieldPath { get; set; }

    [Required]
    [Display(Name = "User Attribute")]
    [UserAttributeReference]
    public int? UserAttributeId { get; set; }
}
