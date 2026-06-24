using Mothenticate.Data.Entities;

namespace Mothenticate.Data.Repositories;

public interface IUserPropertyRepository
{
    Task<IReadOnlyList<UserProperty>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<UserProperty?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<UserProperty> CreateAsync(UserProperty property, CancellationToken cancellationToken = default);
    Task UpdateAsync(UserProperty property, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserPropertyValue>> GetValuesForUserAsync(string userId, CancellationToken cancellationToken = default);
    Task SetValueAsync(string userId, int propertyId, string? value, CancellationToken cancellationToken = default);
}
