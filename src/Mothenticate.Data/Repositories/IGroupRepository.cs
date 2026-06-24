using Mothenticate.Data.Entities;

namespace Mothenticate.Data.Repositories;

public interface IGroupRepository
{
    Task<Group?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Group>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Group> CreateAsync(Group group, CancellationToken cancellationToken = default);
    Task UpdateAsync(Group group, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task AddMemberAsync(string userId, int groupId, CancellationToken cancellationToken = default);
    Task RemoveMemberAsync(string userId, int groupId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ApplicationUser>> GetMembersAsync(int groupId, CancellationToken cancellationToken = default);
}
