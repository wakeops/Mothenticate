using Mothenticate.Data.Entities;

namespace Mothenticate.UserManagement.Services;

public interface IUserAttributeService
{
    Task<IReadOnlyList<UserAttribute>> GetAllDefinitionsAsync(CancellationToken cancellationToken = default);
    Task<UserAttribute?> GetDefinitionByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<UserAttribute> CreateDefinitionAsync(UserAttribute attribute, CancellationToken cancellationToken = default);
    Task UpdateDefinitionAsync(UserAttribute attribute, CancellationToken cancellationToken = default);
    Task DeleteDefinitionAsync(int id, CancellationToken cancellationToken = default);
    Task ReorderAsync(IReadOnlyList<(int Id, int SortOrder)> order, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserAttributeValue>> GetUserValuesAsync(string userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserAttribute>> GetAllWithUserValuesAsync(string userId, CancellationToken cancellationToken = default);
    Task SetUserValuesAsync(string userId, int attributeId, IReadOnlyList<string?> values, CancellationToken cancellationToken = default);
    Task SetUserValueByNameAsync(string userId, string attributeName, string? value, CancellationToken cancellationToken = default);
}
