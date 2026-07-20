namespace Mothenticate.Data.Entities;

public class IdentityProviderMapper
{
    public int Id { get; set; }
    public int IdentityProviderId { get; set; }
    public required string Name { get; set; }
    public SyncMode SyncMode { get; set; } = SyncMode.Inherit;
    public IdentityProviderMapperType MapperType { get; set; }
    public string Config { get; set; } = "{}";

    public IdentityProvider IdentityProvider { get; set; } = null!;
}
