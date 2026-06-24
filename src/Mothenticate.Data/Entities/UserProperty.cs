namespace Mothenticate.Data.Entities;

public class UserProperty
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string DisplayName { get; set; }
    public PropertyType Type { get; set; } = PropertyType.Text;
    public bool IsRequired { get; set; }
    public bool IsHidden { get; set; }
    public bool IsReadOnly { get; set; }
    public int SortOrder { get; set; }

    public ICollection<UserPropertyValue> Values { get; set; } = [];
}
