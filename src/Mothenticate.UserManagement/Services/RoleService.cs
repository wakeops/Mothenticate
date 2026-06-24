using Mothenticate.Data.Entities;
using Mothenticate.Data.Repositories;

namespace Mothenticate.UserManagement.Services;

public class RoleService(IRoleRepository roleRepository) : IRoleService
{
    public Task<AppRole?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => roleRepository.GetByIdAsync(id, cancellationToken);

    public Task<IReadOnlyList<AppRole>> GetAllAsync(CancellationToken cancellationToken = default)
        => roleRepository.GetAllAsync(cancellationToken);

    public Task<AppRole> CreateAsync(string name, string? description = null, CancellationToken cancellationToken = default)
        => roleRepository.CreateAsync(new AppRole { Name = name, Description = description }, cancellationToken);

    public async Task UpdateAsync(int id, string name, string? description = null, CancellationToken cancellationToken = default)
    {
        var role = await roleRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException($"Role '{id}' not found.");

        role.Name = name;
        role.Description = description;
        await roleRepository.UpdateAsync(role, cancellationToken);
    }

    public Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        => roleRepository.DeleteAsync(id, cancellationToken);
}
