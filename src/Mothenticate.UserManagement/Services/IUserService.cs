using Microsoft.AspNetCore.Identity;
using Mothenticate.Data.Entities;

namespace Mothenticate.UserManagement.Services;

public interface IUserService
{
    Task<ApplicationUser?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ApplicationUser>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IdentityResult> CreateAsync(string username, string email, string password);
    Task<IdentityResult> UpdateIdentifiersAsync(string id, string userName, string email, CancellationToken cancellationToken = default);
    Task<IdentityResult> DeleteAsync(string id, CancellationToken cancellationToken = default);
    Task<IdentityResult> ChangePasswordAsync(string id, string currentPassword, string newPassword);
    Task<IdentityResult> SetPasswordAsync(string id, string newPassword);
    Task<IdentityResult> LockAsync(string id);
    Task<IdentityResult> UnlockAsync(string id);
    Task<IdentityResult> SetActiveAsync(string id, bool isActive, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Group>> GetGroupsAsync(string userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AppRole>> GetEffectiveRolesAsync(string userId, CancellationToken cancellationToken = default);
}
