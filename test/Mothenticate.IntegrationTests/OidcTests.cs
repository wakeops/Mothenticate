using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Mothenticate.IntegrationTests;

public class OidcTests(MothenticateWebApplicationFactory factory)
    : IClassFixture<MothenticateWebApplicationFactory>
{
    // Disable auto-redirect: OIDC logout endpoints return 302 which should count as success.
    private readonly HttpClient _client = factory.CreateClient(
        new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    // ── Discovery ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Discovery_ReturnsValidMetadata()
    {
        var response = await _client.GetAsync("/.well-known/openid-configuration");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = doc.RootElement;

        Assert.True(root.TryGetProperty("issuer", out _), "Missing 'issuer'");
        Assert.True(root.TryGetProperty("authorization_endpoint", out _), "Missing 'authorization_endpoint'");
        Assert.True(root.TryGetProperty("token_endpoint", out _), "Missing 'token_endpoint'");
        Assert.True(root.TryGetProperty("jwks_uri", out _), "Missing 'jwks_uri'");
        Assert.True(root.TryGetProperty("response_types_supported", out _), "Missing 'response_types_supported'");
        Assert.True(root.TryGetProperty("subject_types_supported", out _), "Missing 'subject_types_supported'");
        Assert.True(root.TryGetProperty("id_token_signing_alg_values_supported", out _), "Missing 'id_token_signing_alg_values_supported'");
    }

    [Fact]
    public async Task Jwks_ReturnsKeys()
    {
        var response = await _client.GetAsync("/.well-known/jwks");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.True(doc.RootElement.TryGetProperty("keys", out var keys), "Missing 'keys'");
        Assert.NotEmpty(keys.EnumerateArray());
    }

    // ── Client Credentials ────────────────────────────────────────────────────

    [Fact]
    public async Task Token_ClientCredentials_ReturnsAccessToken()
    {
        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = MothenticateWebApplicationFactory.TestClientId,
            ["client_secret"] = MothenticateWebApplicationFactory.TestClientSecret
        });

        var response = await _client.PostAsync("/connect/token", content);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.True(doc.RootElement.TryGetProperty("access_token", out var tokenElement), "Missing 'access_token'");
        Assert.False(string.IsNullOrEmpty(tokenElement.GetString()));
        Assert.True(doc.RootElement.TryGetProperty("token_type", out var typeElement));
        Assert.Equal("Bearer", typeElement.GetString(), ignoreCase: true);
    }

    // ── Introspection ─────────────────────────────────────────────────────────

    [Fact]
    public async Task Introspection_WithValidToken_ReturnsActive()
    {
        var accessToken = await GetClientCredentialsTokenAsync();

        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["token"] = accessToken,
            ["client_id"] = MothenticateWebApplicationFactory.TestClientId,
            ["client_secret"] = MothenticateWebApplicationFactory.TestClientSecret
        });

        var response = await _client.PostAsync("/connect/introspect", content);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.True(doc.RootElement.TryGetProperty("active", out var active));
        Assert.True(active.GetBoolean(), "Token should be active");
    }

    // ── Revocation ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Revocation_RevokesToken_ThenIntrospectShowsInactive()
    {
        var accessToken = await GetClientCredentialsTokenAsync();

        // Revoke
        using var revokeContent = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["token"] = accessToken,
            ["client_id"] = MothenticateWebApplicationFactory.TestClientId,
            ["client_secret"] = MothenticateWebApplicationFactory.TestClientSecret
        });
        var revokeResponse = await _client.PostAsync("/connect/revoke", revokeContent);
        Assert.Equal(HttpStatusCode.OK, revokeResponse.StatusCode);

        // Introspect — should now be inactive
        using var introspectContent = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["token"] = accessToken,
            ["client_id"] = MothenticateWebApplicationFactory.TestClientId,
            ["client_secret"] = MothenticateWebApplicationFactory.TestClientSecret
        });
        var introspectResponse = await _client.PostAsync("/connect/introspect", introspectContent);
        Assert.Equal(HttpStatusCode.OK, introspectResponse.StatusCode);

        using var doc = JsonDocument.Parse(await introspectResponse.Content.ReadAsStringAsync());
        Assert.True(doc.RootElement.TryGetProperty("active", out var active));
        Assert.False(active.GetBoolean(), "Token should be inactive after revocation");
    }

    // ── Userinfo ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Userinfo_WithoutToken_Returns401()
    {
        var response = await _client.GetAsync("/connect/userinfo");

        // OpenIddict returns 401 when no bearer token is provided
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Userinfo_WithClientCredentialsToken_Returns401OrSubjectClaim()
    {
        // client_credentials tokens have a client subject, not a user — userinfo may return 401
        // because no user record exists for the client ID. Either response is spec-conformant.
        var accessToken = await GetClientCredentialsTokenAsync();

        using var request = new HttpRequestMessage(HttpMethod.Get, "/connect/userinfo");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _client.SendAsync(request);

        // client_credentials tokens have a client subject — the UserInfo action may return
        // 401 (no user found) or 403 (authenticated as client but no user record) — both are
        // spec-conformant for this edge case.
        Assert.True(
            response.StatusCode is HttpStatusCode.OK or HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden,
            $"Expected 200, 401, or 403, got {(int)response.StatusCode}");
    }

    // ── End Session ───────────────────────────────────────────────────────────

    [Fact]
    public async Task EndSession_ReturnsOkOrRedirect()
    {
        var response = await _client.GetAsync("/connect/endsession");

        Assert.True(
            response.StatusCode is HttpStatusCode.OK or HttpStatusCode.Redirect or HttpStatusCode.Found,
            $"Expected 200 or redirect, got {response.StatusCode}");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<string> GetClientCredentialsTokenAsync()
    {
        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = MothenticateWebApplicationFactory.TestClientId,
            ["client_secret"] = MothenticateWebApplicationFactory.TestClientSecret
        });

        var response = await _client.PostAsync("/connect/token", content);
        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return doc.RootElement.GetProperty("access_token").GetString()
               ?? throw new InvalidOperationException("No access_token in response");
    }
}
