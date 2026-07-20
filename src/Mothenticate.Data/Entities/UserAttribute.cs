namespace Mothenticate.Data.Entities;

public class UserAttribute
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string DisplayName { get; set; }
    public AttributeInputType InputType { get; set; }
    public bool IsMultivalued { get; set; }
    public string? DefaultValue { get; set; }
    public ScopeCondition EnabledWhen { get; set; } = ScopeCondition.Always;
    public bool IsRequired { get; set; }
    public RequiredFor? RequiredFor { get; set; }
    public ScopeCondition? RequiredWhen { get; set; }
    public bool CanEditUser { get; set; } = true;
    public bool CanEditAdmin { get; set; } = true;
    public bool CanViewUser { get; set; } = true;
    public bool CanViewAdmin { get; set; } = true;
    public bool IsBuiltIn { get; set; }
    public int SortOrder { get; set; }

    public ICollection<UserAttributeValue> Values { get; set; } = [];
    public ICollection<UserAttributeScope> Scopes { get; set; } = [];
    public ICollection<UserAttributeValidator> Validators { get; set; } = [];
}
