using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Mothenticate.Data;
using Mothenticate.Data.Entities;
using Mothenticate.UserManagement.Services;

namespace Mothenticate.IntegrationTests;

public class UserIdentifierStoreTests(MothenticateWebApplicationFactory factory)
    : IClassFixture<MothenticateWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient(
        new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    [Fact]
    public async Task CreateAsync_PersistsIdentifiersToUserAttributeValues_NotAspNetUsersColumns()
    {
        using var scope = factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var db = scope.ServiceProvider.GetRequiredService<MothenticateDbContext>();

        var suffix = Guid.NewGuid().ToString("N")[..8];
        var user = new ApplicationUser
        {
            UserName = $"store-test-{suffix}",
            Email = $"store-test-{suffix}@example.com",
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, "Password1!");
        Assert.True(result.Succeeded, string.Join(", ", result.Errors.Select(e => e.Description)));

        var values = await db.UserAttributeValues
            .Include(v => v.UserAttribute)
            .Where(v => v.UserId == user.Id)
            .ToListAsync();

        Assert.Contains(values, v => v.UserAttribute.Name == "username" && v.Value == user.UserName);
        Assert.Contains(values, v => v.UserAttribute.Name == "email" && v.Value == user.Email);
    }

    [Fact]
    public async Task FindByNameAsync_And_FindByEmailAsync_ResolveViaUserAttributeValues()
    {
        using var scope = factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var suffix = Guid.NewGuid().ToString("N")[..8];
        var username = $"lookup-test-{suffix}";
        var email = $"lookup-test-{suffix}@example.com";

        var created = new ApplicationUser { UserName = username, Email = email, EmailConfirmed = true };
        var createResult = await userManager.CreateAsync(created, "Password1!");
        Assert.True(createResult.Succeeded);

        var byName = await userManager.FindByNameAsync(username);
        Assert.NotNull(byName);
        Assert.Equal(created.Id, byName!.Id);
        Assert.Equal(email, byName.Email);

        var byEmail = await userManager.FindByEmailAsync(email);
        Assert.NotNull(byEmail);
        Assert.Equal(created.Id, byEmail!.Id);
        Assert.Equal(username, byEmail.UserName);
    }

    [Fact]
    public async Task PasswordGrantLogin_And_UserInfo_RoundTripIdentifiersThroughAttributeStore()
    {
        using (var scope = factory.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var userAttributeService = scope.ServiceProvider.GetRequiredService<IUserAttributeService>();
            var clientScopeService = scope.ServiceProvider.GetRequiredService<IClientScopeService>();
            var db = scope.ServiceProvider.GetRequiredService<MothenticateDbContext>();

            var user = new ApplicationUser
            {
                UserName = "password-grant-user",
                Email = "password-grant-user@example.com",
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(user, "Password1!");
            Assert.True(createResult.Succeeded, string.Join(", ", createResult.Errors.Select(e => e.Description)));

            // "displayName" isn't a default-seeded attribute — create it and wire it to the "profile" scope's
            // "name" claim exactly as an admin would via the User Attributes / Client Scopes admin pages, to
            // exercise the mapper-driven claims path end to end.
            var displayNameAttribute = await db.UserAttributes.FirstOrDefaultAsync(a => a.Name == "displayName");
            if (displayNameAttribute is null)
            {
                displayNameAttribute = new UserAttribute
                {
                    Name = "displayName",
                    DisplayName = "Display Name",
                    InputType = AttributeInputType.String
                };
                db.UserAttributes.Add(displayNameAttribute);
                await db.SaveChangesAsync();
            }

            var profileScope = await db.ClientScopes.FirstAsync(s => s.Name == "profile");
            if (!await db.ClientScopeMappers.AnyAsync(m => m.ClientScopeId == profileScope.Id && m.Name == "name"))
            {
                var config = JsonSerializer.Serialize(new Dictionary<string, string>
                {
                    ["TokenClaimName"] = "name",
                    ["UserAttributeId"] = displayNameAttribute.Id.ToString(),
                    ["IncludeAccessToken"] = "true",
                    ["IncludeIdToken"] = "true",
                    ["IncludeIntrospectionToken"] = "true",
                    ["IncludeUserInfo"] = "true"
                });

                await clientScopeService.AddMapperAsync(new ClientScopeMapper
                {
                    ClientScopeId = profileScope.Id,
                    Name = "name",
                    MapperType = MapperType.UserAttribute,
                    Config = config
                });
            }

            await userAttributeService.SetUserValuesAsync(user.Id, displayNameAttribute.Id, ["Password Grant User"]);
        }

        using var tokenContent = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["username"] = "password-grant-user",
            ["password"] = "Password1!",
            ["client_id"] = MothenticateWebApplicationFactory.TestClientId,
            ["client_secret"] = MothenticateWebApplicationFactory.TestClientSecret,
            ["scope"] = "openid profile email acr"
        });

        var tokenResponse = await _client.PostAsync("/connect/token", tokenContent);
        var tokenBody = await tokenResponse.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, tokenResponse.StatusCode);

        using var tokenDoc = JsonDocument.Parse(tokenBody);
        var accessToken = tokenDoc.RootElement.GetProperty("access_token").GetString();
        Assert.False(string.IsNullOrEmpty(accessToken));
        Assert.True(tokenDoc.RootElement.TryGetProperty("id_token", out var idTokenProp), tokenBody);
        var idToken = idTokenProp.GetString();

        using var userInfoRequest = new HttpRequestMessage(HttpMethod.Get, "/connect/userinfo");
        userInfoRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var userInfoResponse = await _client.SendAsync(userInfoRequest);

        Assert.Equal(HttpStatusCode.OK, userInfoResponse.StatusCode);
        using var userInfoDoc = JsonDocument.Parse(await userInfoResponse.Content.ReadAsStringAsync());
        var root = userInfoDoc.RootElement;

        Assert.Equal("password-grant-user@example.com", root.GetProperty("email").GetString());
        Assert.Equal("Password Grant User", root.GetProperty("name").GetString());
        Assert.Equal("true", root.GetProperty("email_verified").GetString());

        // AcrMapper is token-only (no IUserInfoMapper); verify it landed in the actual issued token.
        // Checked via the ID token JWT, not the access token: OpenIddict encrypts access tokens by
        // default (opaque, not a decodable JWT client-side) — id_token is always a plain signed JWT.
        var payload = DecodeJwtPayload(idToken!);
        Assert.Equal("urn:mothenticate:loa:1", payload.GetProperty("acr").GetString());

        // Also verify the same mapped claims surface via /connect/introspect — OpenIddict's built-in
        // introspection handler only ever returns the standard fields (active/sub/iss/...) by default,
        // so this exercises the custom HandleIntrospectionRequestContext handler wired up in
        // Mothenticate.IdentityProvider/Bootstrap.cs that copies over claims destined for introspection.
        using var introspectContent = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["token"] = accessToken!,
            ["client_id"] = MothenticateWebApplicationFactory.TestClientId,
            ["client_secret"] = MothenticateWebApplicationFactory.TestClientSecret
        });

        var introspectResponse = await _client.PostAsync("/connect/introspect", introspectContent);
        Assert.Equal(HttpStatusCode.OK, introspectResponse.StatusCode);

        using var introspectDoc = JsonDocument.Parse(await introspectResponse.Content.ReadAsStringAsync());
        var introspectRoot = introspectDoc.RootElement;

        Assert.True(introspectRoot.GetProperty("active").GetBoolean());
        Assert.Equal("urn:mothenticate:loa:1", introspectRoot.GetProperty("acr").GetString());
    }

    private static JsonElement DecodeJwtPayload(string jwt)
    {
        var payloadSegment = jwt.Split('.')[1];
        var padded = payloadSegment.PadRight(payloadSegment.Length + (4 - payloadSegment.Length % 4) % 4, '=');
        var bytes = Convert.FromBase64String(padded.Replace('-', '+').Replace('_', '/'));
        return JsonDocument.Parse(bytes).RootElement;
    }
}
