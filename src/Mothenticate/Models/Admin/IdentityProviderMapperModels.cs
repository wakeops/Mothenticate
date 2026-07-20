using System.ComponentModel.DataAnnotations;
using Mothenticate.Data.Entities;

namespace Mothenticate.Models.Admin;

public record IdentityProviderMapperEditModel
{
    public int Id { get; init; }
    public int IdentityProviderId { get; init; }
    [Required] public string Name { get; set; } = string.Empty;
    public IdentityProviderMapperType MapperType { get; set; } = IdentityProviderMapperType.AttributeImporter;
    public SyncMode SyncMode { get; set; } = SyncMode.Inherit;
}
