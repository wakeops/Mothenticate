using Mothenticate.Data.Entities;
using Mothenticate.Data.Repositories;

namespace Mothenticate.UserManagement.Services;

public class GroupService(
    IGroupRepository groupRepository,
    IRoleRepository roleRepository) : IGroupService
{
    public Task<Group?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => groupRepository.GetByIdAsync(id, cancellationToken);

    public Task<IReadOnlyList<Group>> GetAllAsync(CancellationToken cancellationToken = default)
        => groupRepository.GetAllAsync(cancellationToken);

    public Task<Group> CreateAsync(string name, string? description = null, CancellationToken cancellationToken = default)
        => groupRepository.CreateAsync(new Group { Name = name, Description = description }, cancellationToken);

    public async Task UpdateAsync(int id, string name, string? description = null, CancellationToken cancellationToken = default)
    {
        var group = await groupRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException($"Group '{id}' not found.");

        group.Name = name;
        group.Description = description;
        await groupRepository.UpdateAsync(group, cancellationToken);
    }

    public Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        => groupRepository.DeleteAsync(id, cancellationToken);

    public Task AddMemberAsync(int groupId, string userId, CancellationToken cancellationToken = default)
        => groupRepository.AddMemberAsync(userId, groupId, cancellationToken);

    public Task RemoveMemberAsync(int groupId, string userId, CancellationToken cancellationToken = default)
        => groupRepository.RemoveMemberAsync(userId, groupId, cancellationToken);

    public Task<IReadOnlyList<ApplicationUser>> GetMembersAsync(int groupId, CancellationToken cancellationToken = default)
        => groupRepository.GetMembersAsync(groupId, cancellationToken);

    public Task AddRoleAsync(int groupId, int roleId, CancellationToken cancellationToken = default)
        => roleRepository.AddToGroupAsync(roleId, groupId, cancellationToken);

    public Task RemoveRoleAsync(int groupId, int roleId, CancellationToken cancellationToken = default)
        => roleRepository.RemoveFromGroupAsync(roleId, groupId, cancellationToken);

    public Task<IReadOnlyList<AppRole>> GetRolesAsync(int groupId, CancellationToken cancellationToken = default)
        => roleRepository.GetByGroupAsync(groupId, cancellationToken);
}
