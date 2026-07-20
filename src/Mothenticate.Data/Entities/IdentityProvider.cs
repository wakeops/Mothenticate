namespace Mothenticate.Data.Entities;

public class IdentityProvider
{
    public int Id { get; set; }

    public int ProviderTypeId { get; set; }
    public IdentityProviderType ProviderType { get; set; } = null!;

    public int? ConfigurationId { get; set; }
    public IdentityProviderConfiguration? Configuration { get; set; }

    public string Alias { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;

    public bool IsEnabled { get; set; } = true;
    public bool AccountLinkingOnly { get; set; } = false;
    public bool HideOnLoginPage { get; set; } = false;
    public ShowInAccountConsole ShowInAccountConsole { get; set; } = ShowInAccountConsole.Always;
    public SyncMode SyncMode { get; set; } = SyncMode.Import;

    public int? DisplayOrder { get; set; }

    public ICollection<IdentityProviderMapper> Mappers { get; set; } = [];
}
