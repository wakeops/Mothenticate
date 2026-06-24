using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Mothenticate.Data.Entities;
using Mothenticate.Domain.Config;

namespace Mothenticate.Data.Services;

public class SeedService(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    ILogger<SeedService> logger) : ISeedService
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await EnsureAdminRoleAsync();
    }

    public async Task<bool> IsFirstRunAsync(CancellationToken cancellationToken = default)
    {
        var admins = await userManager.GetUsersInRoleAsync(AuthDefaults.AppAdminUserRole);
        return admins.Count == 0;
    }

    private async Task EnsureAdminRoleAsync()
    {
        if (!await roleManager.RoleExistsAsync(AuthDefaults.AppAdminUserRole))
        {
            var result = await roleManager.CreateAsync(new IdentityRole(AuthDefaults.AppAdminUserRole));
            if (result.Succeeded)
                logger.LogInformation("Created admin role '{Role}'", AuthDefaults.AppAdminUserRole);
            else
                logger.LogError("Failed to create admin role: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }
}
