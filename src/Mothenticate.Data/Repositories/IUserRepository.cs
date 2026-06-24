using Mothenticate.Data.Entities;

namespace Mothenticate.Data.Repositories;

public interface IUserRepository
{
    Task<ApplicationUser?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<ApplicationUser?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ApplicationUser>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ApplicationUser>> GetByGroupAsync(int groupId, CancellationToken cancellationToken = default);
}
