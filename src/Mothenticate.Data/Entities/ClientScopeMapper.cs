namespace Mothenticate.Data.Entities;

public class ClientScopeMapper
{
    public int Id { get; set; }
    public int ClientScopeId { get; set; }
    public required string Name { get; set; }
    public MapperType MapperType { get; set; }
    public string Config { get; set; } = "{}";

    // Destinations the mapper's claim(s) are added to — generic per-mapper output routing, not part of
    // the mapper's own settings, so they live as dedicated columns rather than keys inside Config.
    public bool IncludeAccessToken { get; set; } = true;
    public bool IncludeIdToken { get; set; } = true;
    public bool IncludeIntrospectionToken { get; set; } = true;
    public bool IncludeUserInfo { get; set; } = true;

    public ClientScope ClientScope { get; set; } = null!;
}
