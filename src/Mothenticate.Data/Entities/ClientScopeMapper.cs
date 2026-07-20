namespace Mothenticate.Data.Entities;

public class ClientScopeMapper
{
    public int Id { get; set; }
    public int ClientScopeId { get; set; }
    public required string Name { get; set; }
    public MapperType MapperType { get; set; }
    public string Config { get; set; } = "{}";

    public ClientScope ClientScope { get; set; } = null!;
}
