namespace Mothenticate.IdentityProvider.Services.ScopeMappers.Abstract;

/// <summary>
/// Marks a settings model property as an FK reference to <c>UserAttribute.Id</c> — the admin UI renders
/// it as a picker over the available user attributes instead of a plain number field.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class UserAttributeReferenceAttribute : Attribute;
