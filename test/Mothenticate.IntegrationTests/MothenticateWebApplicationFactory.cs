using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Mothenticate.Data;
using OpenIddict.Abstractions;

namespace Mothenticate.IntegrationTests;

public class MothenticateWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string TestClientId = "test-client";
    public const string TestClientSecret = "test-secret-value";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // Fix the database name here so it stays the same across all DI scopes.
        // Generating the GUID inside the AddDbContext lambda would call NewGuid() once
        // per scope resolution, giving every scope a different in-memory store.
        var dbName = "TestDb-" + Guid.NewGuid();

        builder.ConfigureServices(services =>
        {
            // Remove ALL service descriptors related to MothenticateDbContext.
            // This includes EF Core's internal IDbContextOptionsConfiguration<T>
            // (registered by AddDbContext) as well as the top-level options descriptor.
            // Both Npgsql AND InMemory providers would otherwise end up in the same
            // internal EF Core service provider, which throws at context creation time.
            var toRemove = services
                .Where(d => d.ServiceType == typeof(MothenticateDbContext)
                         || d.ServiceType == typeof(DbContextOptions<MothenticateDbContext>)
                         || (d.ServiceType.IsGenericType
                             && d.ServiceType.GenericTypeArguments.Contains(typeof(MothenticateDbContext))))
                .ToList();

            foreach (var d in toRemove)
            {
                services.Remove(d);
            }

            services.AddDbContext<MothenticateDbContext>(options =>
                options.UseInMemoryDatabase(dbName));
        });
    }

    public async ValueTask InitializeAsync()
    {
        // Ensure schema is created and seed a test OIDC client
        using var scope = Services.CreateScope();
        var sp = scope.ServiceProvider;

        var db = sp.GetRequiredService<MothenticateDbContext>();
        await db.Database.EnsureCreatedAsync();

        var appManager = sp.GetRequiredService<IOpenIddictApplicationManager>();

        if (await appManager.FindByClientIdAsync(TestClientId) is null)
        {
            await appManager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = TestClientId,
                ClientType = OpenIddictConstants.ClientTypes.Confidential,
                ClientSecret = TestClientSecret,
                DisplayName = "Integration Test Client",
                Permissions =
                {
                    OpenIddictConstants.Permissions.Endpoints.Token,
                    OpenIddictConstants.Permissions.Endpoints.Introspection,
                    OpenIddictConstants.Permissions.Endpoints.Revocation,
                    OpenIddictConstants.Permissions.GrantTypes.ClientCredentials,
                    OpenIddictConstants.Permissions.GrantTypes.Password,
                    OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                    OpenIddictConstants.Permissions.Prefixes.Scope + OpenIddictConstants.Scopes.OpenId,
                    OpenIddictConstants.Permissions.Prefixes.Scope + OpenIddictConstants.Scopes.Email,
                    OpenIddictConstants.Permissions.Prefixes.Scope + OpenIddictConstants.Scopes.Profile,
                    OpenIddictConstants.Permissions.ResponseTypes.Token
                },
                Properties =
                {
                    ["mothenticate:is_enabled"] = System.Text.Json.JsonSerializer.SerializeToElement(true)
                }
            });
        }
    }

    public new ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
