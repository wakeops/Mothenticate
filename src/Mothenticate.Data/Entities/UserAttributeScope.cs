namespace Mothenticate.Data.Entities;

public class UserAttributeScope
{
    public int Id { get; set; }
    public int UserAttributeId { get; set; }
    public int ScopeId { get; set; }
    public ScopePurpose Purpose { get; set; }

    public UserAttribute UserAttribute { get; set; } = null!;
    public ClientScope Scope { get; set; } = null!;
}
