namespace Mothenticate.Data.Entities;

public class UserAttributeValidator
{
    public int Id { get; set; }
    public int UserAttributeId { get; set; }
    public ValidatorType ValidatorType { get; set; }
    public string ConfigJson { get; set; } = "{}";

    public UserAttribute UserAttribute { get; set; } = null!;
}
