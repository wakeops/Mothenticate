using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Mothenticate.Data.Entities;
using Mothenticate.Data.EntityMappings;

namespace Mothenticate.Data;

public class MothenticateDbContext(DbContextOptions<MothenticateDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<UserProperty> UserProperties => Set<UserProperty>();
    public DbSet<UserPropertyValue> UserPropertyValues => Set<UserPropertyValue>();
    public DbSet<Group> Groups => Set<Group>();
    public DbSet<AppRole> AppRoles => Set<AppRole>();
    public DbSet<GroupRole> GroupRoles => Set<GroupRole>();
    public DbSet<UserGroup> UserGroups => Set<UserGroup>();
    public DbSet<UserSession> UserSessions => Set<UserSession>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<AppSettings> AppSettings => Set<AppSettings>();
    public DbSet<ClientSecret> ClientSecrets => Set<ClientSecret>();
    public DbSet<AppLauncher> AppLaunchers => Set<AppLauncher>();
    public DbSet<AppLauncherAccess> AppLauncherAccess => Set<AppLauncherAccess>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.UseOpenIddict();

        builder.ApplyConfiguration(new ApplicationUserMapping());
        builder.ApplyConfiguration(new UserPropertyMapping());
        builder.ApplyConfiguration(new UserPropertyValueMapping());
        builder.ApplyConfiguration(new GroupMapping());
        builder.ApplyConfiguration(new AppRoleMapping());
        builder.ApplyConfiguration(new GroupRoleMapping());
        builder.ApplyConfiguration(new UserGroupMapping());
        builder.ApplyConfiguration(new UserSessionMapping());
        builder.ApplyConfiguration(new AuditLogMapping());
        builder.ApplyConfiguration(new AppSettingsMapping());
        builder.ApplyConfiguration(new ClientSecretMapping());
        builder.ApplyConfiguration(new AppLauncherMapping());
        builder.ApplyConfiguration(new AppLauncherAccessMapping());
    }
}
