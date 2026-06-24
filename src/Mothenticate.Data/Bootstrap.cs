using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Mothenticate.Data.Entities;
using Mothenticate.Data.Repositories;
using Mothenticate.Data.Services;
using Mothenticate.Domain.Config;

namespace Mothenticate.Data;

public static class Bootstrap
{
    public static IServiceCollection AddAppData(this IServiceCollection services, AppConfigDatabase dbConfig)
    {
        services.AddDbContext<MothenticateDbContext>(options =>
            options.UseNpgsql(dbConfig.GetConnectionString()));

        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<MothenticateDbContext>()
            .AddTokenProvider<AuthenticatorTokenProvider<ApplicationUser>>(TokenOptions.DefaultAuthenticatorProvider);

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IGroupRepository, GroupRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IUserPropertyRepository, UserPropertyRepository>();
        services.AddScoped<ISessionRepository, SessionRepository>();
        services.AddScoped<IAuditRepository, AuditRepository>();
        services.AddScoped<IAppSettingsRepository, AppSettingsRepository>();
        services.AddScoped<IClientSecretRepository, ClientSecretRepository>();
        services.AddScoped<IAppLauncherRepository, AppLauncherRepository>();

        services.AddScoped<ISeedService, SeedService>();
        services.AddSingleton<SetupState>();

        return services;
    }

    public static async Task RunAppDataMigrationsAsync(this IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<MothenticateDbContext>();

        if (db.Database.IsRelational())
        {
            await db.Database.MigrateAsync();
        }
        else
        {
            await db.Database.EnsureCreatedAsync();
        }

        var seedService = scope.ServiceProvider.GetRequiredService<ISeedService>();
        await seedService.SeedAsync();
    }
}
