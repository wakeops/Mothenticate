using System.ComponentModel.DataAnnotations;
using Mothenticate.Data.Entities;

namespace Mothenticate.Models.Admin;

public record ClientScopeEditModel
{
    public int Id { get; init; }
    [Required] public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public AssignedScopeType AssignedType { get; set; } = AssignedScopeType.None;
    public ProtocolType Protocol { get; set; } = ProtocolType.OIDC;
    public bool DisplayOnConsentScreen { get; set; }
    public string? ConsentText { get; set; }
    public bool IncludeInTokenScope { get; set; }
    public bool IncludeInMetadata { get; set; } = true;
}

public record ClientScopeMapperEditModel
{
    public int Id { get; init; }
    public int ClientScopeId { get; init; }
    [Required] public string Name { get; set; } = string.Empty;
    public MapperType MapperType { get; set; } = MapperType.UserAttribute;
    public bool IncludeAccessToken { get; set; } = true;
    public bool IncludeIdToken { get; set; } = true;
    public bool IncludeIntrospectionToken { get; set; } = true;
    public bool IncludeUserInfo { get; set; } = true;
}
