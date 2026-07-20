using Mothenticate.Data.Entities;

namespace Mothenticate.Data.Repositories;

public interface IUserAttributeRepository
{
    Task<IReadOnlyList<UserAttribute>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<UserAttribute?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<UserAttribute> CreateAsync(UserAttribute attribute, CancellationToken cancellationToken = default);
    Task UpdateAsync(UserAttribute attribute, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task UpdateSortOrderAsync(IReadOnlyList<(int Id, int SortOrder)> order, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserAttributeValue>> GetValuesForUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserAttribute>> GetAllWithUserValuesAsync(string userId, CancellationToken cancellationToken = default);
    Task SetValuesAsync(string userId, int attributeId, IReadOnlyList<string?> values, CancellationToken cancellationToken = default);
}
