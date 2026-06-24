using Mothenticate.Data.Entities;

namespace Mothenticate.Data.Repositories;

public interface IRoleRepository
{
    Task<AppRole?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AppRole>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<AppRole> CreateAsync(AppRole role, CancellationToken cancellationToken = default);
    Task UpdateAsync(AppRole role, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task AddToGroupAsync(int roleId, int groupId, CancellationToken cancellationToken = default);
    Task RemoveFromGroupAsync(int roleId, int groupId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AppRole>> GetByGroupAsync(int groupId, CancellationToken cancellationToken = default);
}
