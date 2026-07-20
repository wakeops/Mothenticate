using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Mothenticate.Data.Entities;
using Mothenticate.Data.EntityMappings;

namespace Mothenticate.Data;

public class MothenticateDbContext(DbContextOptions<MothenticateDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<UserAttribute> UserAttributes => Set<UserAttribute>();
    public DbSet<UserAttributeValue> UserAttributeValues => Set<UserAttributeValue>();
    public DbSet<UserAttributeScope> UserAttributeScopes => Set<UserAttributeScope>();
    public DbSet<UserAttributeValidator> UserAttributeValidators => Set<UserAttributeValidator>();
    public DbSet<ClientScope> ClientScopes => Set<ClientScope>();
    public DbSet<ClientScopeMapper> ClientScopeMappers => Set<ClientScopeMapper>();
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
    public DbSet<IdentityProviderType> IdentityProviderTypes => Set<IdentityProviderType>();
    public DbSet<IdentityProviderConfiguration> IdentityProviderConfigurations => Set<IdentityProviderConfiguration>();
    public DbSet<IdentityProvider> IdentityProviders => Set<IdentityProvider>();
    public DbSet<IdentityProviderMapper> IdentityProviderMappers => Set<IdentityProviderMapper>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.UseOpenIddict();

        builder.ApplyConfiguration(new ApplicationUserMapping());
        builder.ApplyConfiguration(new UserAttributeMapping());
        builder.ApplyConfiguration(new UserAttributeValueMapping());
        builder.ApplyConfiguration(new UserAttributeScopeMapping());
        builder.ApplyConfiguration(new UserAttributeValidatorMapping());
        builder.ApplyConfiguration(new ClientScopeMapping());
        builder.ApplyConfiguration(new ClientScopeMapperMapping());
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
        builder.ApplyConfiguration(new IdentityProviderConfigurationMapping());
        builder.ApplyConfiguration(new IdentityProviderTypeMapping());
        builder.ApplyConfiguration(new IdentityProviderMapping());
        builder.ApplyConfiguration(new IdentityProviderMapperMapping());
    }
}
