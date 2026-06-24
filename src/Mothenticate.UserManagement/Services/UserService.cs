using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Mothenticate.Data.Entities;
using Mothenticate.Data.Repositories;

namespace Mothenticate.UserManagement.Services;

public class UserService(
    UserManager<ApplicationUser> userManager,
    IUserRepository userRepository,
    IGroupRepository groupRepository,
    IRoleRepository roleRepository,
    IAppSettingsService appSettingsService,
    ILogger<UserService> logger) : IUserService
{
    public Task<ApplicationUser?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        => userRepository.GetByIdAsync(id, cancellationToken);

    public Task<IReadOnlyList<ApplicationUser>> GetAllAsync(CancellationToken cancellationToken = default)
        => userRepository.GetAllAsync(cancellationToken);

    public async Task<IdentityResult> CreateAsync(string username, string email, string password,
        string? firstName = null, string? lastName = null, string? displayName = null)
    {
        var settings = await appSettingsService.GetAsync();
        var user = new ApplicationUser
        {
            UserName = settings.UseEmailAsUsername ? email : username,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            DisplayName = displayName,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            logger.LogWarning("Failed to create user '{Username}': {Errors}", username, string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        return result;
    }

    public async Task<IdentityResult> UpdateProfileAsync(string id, string? firstName, string? lastName, string? displayName, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null)
        {
            return IdentityResult.Failed(new IdentityError { Code = "UserNotFound", Description = $"User '{id}' not found." });
        }

        user.FirstName = firstName;
        user.LastName = lastName;
        user.DisplayName = displayName;

        return await userManager.UpdateAsync(user);
    }

    public async Task<IdentityResult> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null)
        {
            return IdentityResult.Failed(new IdentityError { Code = "UserNotFound", Description = $"User '{id}' not found." });
        }

        return await userManager.DeleteAsync(user);
    }

    public async Task<IdentityResult> ChangePasswordAsync(string id, string currentPassword, string newPassword)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null)
        {
            return IdentityResult.Failed(new IdentityError { Code = "UserNotFound", Description = $"User '{id}' not found." });
        }

        return await userManager.ChangePasswordAsync(user, currentPassword, newPassword);
    }

    public async Task<IdentityResult> SetPasswordAsync(string id, string newPassword)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null)
        {
            return IdentityResult.Failed(new IdentityError { Code = "UserNotFound", Description = $"User '{id}' not found." });
        }

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        return await userManager.ResetPasswordAsync(user, token, newPassword);
    }

    public async Task<IdentityResult> LockAsync(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null)
        {
            return IdentityResult.Failed(new IdentityError { Code = "UserNotFound", Description = $"User '{id}' not found." });
        }

        return await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
    }

    public async Task<IdentityResult> UnlockAsync(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null)
        {
            return IdentityResult.Failed(new IdentityError { Code = "UserNotFound", Description = $"User '{id}' not found." });
        }

        await userManager.ResetAccessFailedCountAsync(user);
        return await userManager.SetLockoutEndDateAsync(user, null);
    }

    public async Task<IdentityResult> SetActiveAsync(string id, bool isActive, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null)
        {
            return IdentityResult.Failed(new IdentityError { Code = "UserNotFound", Description = $"User '{id}' not found." });
        }

        user.IsActive = isActive;
        return await userManager.UpdateAsync(user);
    }

    public async Task<IdentityResult> SetAvatarAsync(string id, byte[] data, string contentType, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null)
        {
            return IdentityResult.Failed(new IdentityError { Code = "UserNotFound", Description = $"User '{id}' not found." });
        }

        user.AvatarData = data;
        user.AvatarContentType = contentType;
        return await userManager.UpdateAsync(user);
    }

    public async Task<IdentityResult> RemoveAvatarAsync(string id, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null)
        {
            return IdentityResult.Failed(new IdentityError { Code = "UserNotFound", Description = $"User '{id}' not found." });
        }

        user.AvatarData = null;
        user.AvatarContentType = null;
        return await userManager.UpdateAsync(user);
    }

    public async Task<IReadOnlyList<Group>> GetGroupsAsync(string userId, CancellationToken cancellationToken = default)
    {
        var allGroups = await groupRepository.GetAllAsync(cancellationToken);
        var members = await Task.WhenAll(allGroups.Select(async g =>
            (Group: g, IsMember: (await groupRepository.GetMembersAsync(g.Id, cancellationToken)).Any(u => u.Id == userId))));
        return members.Where(x => x.IsMember).Select(x => x.Group).ToList();
    }

    public async Task<IReadOnlyList<AppRole>> GetEffectiveRolesAsync(string userId, CancellationToken cancellationToken = default)
    {
        var groups = await GetGroupsAsync(userId, cancellationToken);
        var roles = await Task.WhenAll(groups.Select(g => roleRepository.GetByGroupAsync(g.Id, cancellationToken)));
        return roles.SelectMany(r => r).DistinctBy(r => r.Id).ToList();
    }
}
