using Microsoft.Extensions.DependencyInjection;
using Mothenticate.UserManagement.Services;

namespace Mothenticate.UserManagement;

public static class Bootstrap
{
    public static IServiceCollection AddUserManagement(this IServiceCollection services)
    {
        services.AddScoped<IAppSettingsService, AppSettingsService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IGroupService, GroupService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IUserPropertyService, UserPropertyService>();
        services.AddScoped<IAppLauncherService, AppLauncherService>();

        return services;
    }
}
