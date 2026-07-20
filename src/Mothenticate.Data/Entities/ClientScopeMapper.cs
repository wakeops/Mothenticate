namespace Mothenticate.Data.Entities;

public class ClientScopeMapper
{
    public int Id { get; set; }
    public int ClientScopeId { get; set; }
    public required string Name { get; set; }
    public MapperType MapperType { get; set; }
    public bool IncludeAccessToken { get; set; }
    public bool IncludeIdToken { get; set; }
    public bool IncludeIntrospectionToken { get; set; }
    public bool IncludeUserInfo { get; set; }
    public string Config { get; set; } = "{}";
    
    public ClientScope ClientScope { get; set; } = null!;
}
