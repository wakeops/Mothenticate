namespace Mothenticate.Data.Entities;

public class ClientScope
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public AssignedScopeType AssignedType { get; set; } = AssignedScopeType.None;
    public ProtocolType Protocol { get; set; }
    public bool DisplayOnConsentScreen { get; set; }
    public string? ConsentText { get; set; }
    public bool IncludeInTokenScope { get; set; }
    public bool IncludeInMetadata { get; set; } = true;

    public ICollection<ClientScopeMapper> Mappers { get; set; } = [];
    public ICollection<UserAttributeScope> AttributeScopes { get; set; } = [];
}
