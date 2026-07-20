namespace Mothenticate.Data.Entities;

public enum SyncMode
{
    Import,
    Update,

    /// <summary>
    /// Mapper-only value — defers to the owning IdentityProvider's SyncMode. Not offered as an option
    /// on IdentityProvider.SyncMode itself, since there's nothing above it to inherit from.
    /// </summary>
    Inherit,
}
