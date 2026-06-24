using Mothenticate.Data.Entities;

namespace Mothenticate.UserManagement.Services;

public interface IGroupService
{
    Task<Group?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Group>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Group> CreateAsync(string name, string? description = null, CancellationToken cancellationToken = default);
    Task UpdateAsync(int id, string name, string? description = null, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task AddMemberAsync(int groupId, string userId, CancellationToken cancellationToken = default);
    Task RemoveMemberAsync(int groupId, string userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ApplicationUser>> GetMembersAsync(int groupId, CancellationToken cancellationToken = default);
    Task AddRoleAsync(int groupId, int roleId, CancellationToken cancellationToken = default);
    Task RemoveRoleAsync(int groupId, int roleId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AppRole>> GetRolesAsync(int groupId, CancellationToken cancellationToken = default);
}
