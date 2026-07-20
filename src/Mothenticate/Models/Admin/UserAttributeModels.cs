using System.ComponentModel.DataAnnotations;
using Mothenticate.Data.Entities;

namespace Mothenticate.Models.Admin;

public record UserAttributeScopeModel
{
    public int ScopeId { get; init; }
    public string ScopeName { get; init; } = string.Empty;
}

public record UserAttributeValidatorModel
{
    public int Id { get; init; }
    public ValidatorType ValidatorType { get; init; }
    public string ConfigJson { get; set; } = "{}";
}

public record UserAttributeEditModel
{
    public int Id { get; init; }
    [Required] public string Name { get; set; } = string.Empty;
    [Required] public string DisplayName { get; set; } = string.Empty;
    public AttributeInputType? InputType { get; set; }
    public bool IsMultivalued { get; set; }
    public string? DefaultValue { get; set; }
    public ScopeCondition EnabledWhen { get; set; } = ScopeCondition.Always;
    public List<int> EnabledWhenScopeIds { get; set; } = [];
    public bool IsRequired { get; set; }
    public RequiredFor RequiredFor { get; set; } = RequiredFor.Both;
    public ScopeCondition RequiredWhen { get; set; } = ScopeCondition.Always;
    public List<int> RequiredWhenScopeIds { get; set; } = [];
    public bool CanEditUser { get; set; } = true;
    public bool CanEditAdmin { get; set; } = true;
    public bool CanViewUser { get; set; } = true;
    public bool CanViewAdmin { get; set; } = true;
    public bool IsBuiltIn { get; init; }
    public List<UserAttributeValidatorModel> Validators { get; set; } = [];
}
