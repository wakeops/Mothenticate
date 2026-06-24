using Mothenticate.Data.Entities;

namespace Mothenticate.UserManagement.Services;

public interface IRoleService
{
    Task<AppRole?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AppRole>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<AppRole> CreateAsync(string name, string? description = null, CancellationToken cancellationToken = default);
    Task UpdateAsync(int id, string name, string? description = null, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
