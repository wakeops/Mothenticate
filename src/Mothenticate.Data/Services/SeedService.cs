using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mothenticate.Data.Entities;
using Mothenticate.Data.Repositories;
using Mothenticate.Domain.Config;

namespace Mothenticate.Data.Services;

public class SeedService(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IAppSettingsRepository appSettingsRepository,
    MothenticateDbContext db,
    ILogger<SeedService> logger) : ISeedService
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await EnsureAdminRoleAsync();
        await appSettingsRepository.GetAsync(cancellationToken); // ensures singleton row exists
        await EnsureDefaultUserAttributesAsync(cancellationToken);
        await EnsureDefaultClientScopesAsync(cancellationToken);
        await EnsureDefaultClientScopeMappersAsync(cancellationToken);

        logger.LogInformation("Seeded initial data successfully.");
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

    private async Task EnsureDefaultUserAttributesAsync(CancellationToken cancellationToken)
    {
        if (await db.UserAttributes.AnyAsync(cancellationToken))
        {
            return;
        }

        db.UserAttributes.AddRange(
            new UserAttribute { Name = "username", DisplayName = "Username", InputType = AttributeInputType.String, IsRequired = true, IsBuiltIn = true, CanEditUser = false, SortOrder = 0 },
            new UserAttribute { Name = "email", DisplayName = "Email", InputType = AttributeInputType.String, IsRequired = true, IsBuiltIn = true, CanEditUser = false, SortOrder = 1 },
            new UserAttribute { Name = "firstName", DisplayName = "First Name", InputType = AttributeInputType.String, IsBuiltIn = false, SortOrder = 2 },
            new UserAttribute { Name = "lastName", DisplayName = "Last Name", InputType = AttributeInputType.String, IsBuiltIn = false, SortOrder = 3 }
        );

        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureDefaultClientScopesAsync(CancellationToken cancellationToken)
    {
        if (await db.ClientScopes.AnyAsync(cancellationToken))
        {
            return;
        }

        db.ClientScopes.AddRange(
            new ClientScope { Name = "acr", Description = "OpenID Connect built-in acr scope", AssignedType = AssignedScopeType.Default, Protocol = ProtocolType.OIDC, DisplayOnConsentScreen = false, IncludeInTokenScope = false, IncludeInMetadata = true },
            new ClientScope { Name = "basic", Description = "OpenID Connect built-in basic scope", AssignedType = AssignedScopeType.Default, Protocol = ProtocolType.OIDC, DisplayOnConsentScreen = false, IncludeInTokenScope = false, IncludeInMetadata = true },
            new ClientScope { Name = "email", Description = "OpenID Connect built-in email scope", AssignedType = AssignedScopeType.Default, Protocol = ProtocolType.OIDC, DisplayOnConsentScreen = true, IncludeInTokenScope = true, IncludeInMetadata = true },
            new ClientScope { Name = "profile", Description = "OpenID Connect built-in profile scope", AssignedType = AssignedScopeType.Default, Protocol = ProtocolType.OIDC, DisplayOnConsentScreen = true, IncludeInTokenScope = true, IncludeInMetadata = true },
            new ClientScope { Name = "roles", Description = "OpenID Connect built-in roles scope", AssignedType = AssignedScopeType.Default, Protocol = ProtocolType.OIDC, DisplayOnConsentScreen = true, IncludeInTokenScope = true, IncludeInMetadata = true },
            new ClientScope { Name = "offline_access", Description = "OpenID Connect built-in offline_access scope", AssignedType = AssignedScopeType.Optional, Protocol = ProtocolType.OIDC, DisplayOnConsentScreen = true, IncludeInTokenScope = true, IncludeInMetadata = true }
        );

        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureDefaultClientScopeMappersAsync(CancellationToken cancellationToken)
    {
        if (await db.ClientScopeMappers.AnyAsync(cancellationToken))
        {
            return;
        }

        var scopesByName = await db.ClientScopes.ToDictionaryAsync(s => s.Name, cancellationToken);
        var attributesByName = await db.UserAttributes.ToDictionaryAsync(a => a.Name, cancellationToken);
        var mappers = new List<ClientScopeMapper>();

        void AddAttributeMapper(string scopeName, string attributeName, string claimName)
        {
            if (scopesByName.TryGetValue(scopeName, out var scope) && attributesByName.TryGetValue(attributeName, out var attribute))
            {
                var config = new Dictionary<string, string> { ["TokenClaimName"] = claimName, ["UserAttributeId"] = attribute.Id.ToString() };
                mappers.Add(new ClientScopeMapper
                {
                    ClientScopeId = scope.Id,
                    Name = claimName,
                    MapperType = MapperType.UserAttribute,
                    Config = JsonSerializer.Serialize(config),
                    IncludeAccessToken = true,
                    IncludeIdToken = true,
                    IncludeIntrospectionToken = false,
                    IncludeUserInfo = true
                });
            }
        }

        void AddPropertyMapper(string scopeName, UserPropertyField field, string claimName)
        {
            if (scopesByName.TryGetValue(scopeName, out var scope))
            {
                var config = new Dictionary<string, string> { ["TokenClaimName"] = claimName, ["UserProperty"] = field.ToString() };
                mappers.Add(new ClientScopeMapper
                {
                    ClientScopeId = scope.Id,
                    Name = claimName,
                    MapperType = MapperType.UserProperty,
                    Config = JsonSerializer.Serialize(config),
                    IncludeAccessToken = true,
                    IncludeIdToken = true,
                    IncludeIntrospectionToken = true,
                    IncludeUserInfo = true
                });
            }
        }

        void AddContextMapper(string scopeName, string claimName)
        {
            if (scopesByName.TryGetValue(scopeName, out var scope))
            {
                mappers.Add(new ClientScopeMapper
                {
                    ClientScopeId = scope.Id,
                    Name = claimName,
                    MapperType = MapperType.AuthenticationContextReference,
                    IncludeAccessToken = true,
                    IncludeIdToken = true,
                    IncludeIntrospectionToken = true,
                    IncludeUserInfo = true
                });
            }
        }

        // Note: "sub" is intentionally not seeded here — AuthorizationController adds it
        // unconditionally regardless of scope (mandatory per OIDC), and also registering it via
        // SubjectMapper would risk a duplicate claim. SubjectMapper stays available in the
        // registry for scopes that want to reference it explicitly.

        AddAttributeMapper("email", "email", "email");
        AddPropertyMapper("email", UserPropertyField.EmailConfirmed, "email_verified");
        AddAttributeMapper("profile", "firstName", "given_name");
        AddAttributeMapper("profile", "lastName", "family_name");
        AddAttributeMapper("profile", "username", "preferred_username");
        AddContextMapper("acr", "acr");

        if (mappers.Count == 0)
        {
            return;
        }

        db.ClientScopeMappers.AddRange(mappers);
        await db.SaveChangesAsync(cancellationToken);
    }
}
