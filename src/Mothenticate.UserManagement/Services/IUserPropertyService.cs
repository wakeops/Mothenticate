using Mothenticate.Data.Entities;

namespace Mothenticate.UserManagement.Services;

public interface IUserPropertyService
{
    Task<IReadOnlyList<UserProperty>> GetAllDefinitionsAsync(CancellationToken cancellationToken = default);
    Task<UserProperty?> GetDefinitionByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<UserProperty> CreateDefinitionAsync(string name, string displayName, PropertyType type, bool isRequired = false, bool isHidden = false, bool isReadOnly = false, CancellationToken cancellationToken = default);
    Task UpdateDefinitionAsync(int id, string displayName, PropertyType type, bool isRequired, bool isHidden, bool isReadOnly, CancellationToken cancellationToken = default);
    Task DeleteDefinitionAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserPropertyValue>> GetUserValuesAsync(string userId, CancellationToken cancellationToken = default);
    Task SetUserValueAsync(string userId, int propertyId, string? value, CancellationToken cancellationToken = default);
}
