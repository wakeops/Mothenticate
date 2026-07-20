namespace Mothenticate.Data.Entities;

public class IdentityProviderType
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ProtocolType ProtocolType { get; set; }

    public int? DefaultConfigurationId { get; set; }
    public IdentityProviderConfiguration? DefaultConfiguration { get; set; }

    public ICollection<IdentityProvider> Providers { get; set; } = [];
}
