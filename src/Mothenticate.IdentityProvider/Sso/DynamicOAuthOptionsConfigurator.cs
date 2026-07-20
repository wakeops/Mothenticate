using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Mothenticate.Data.Repositories;
using IdpEntity = Mothenticate.Data.Entities.IdentityProvider;

namespace Mothenticate.IdentityProvider.Sso;

public sealed class DynamicOAuthOptionsConfigurator(IServiceScopeFactory scopeFactory)
    : IConfigureNamedOptions<OAuthOptions>
{
    public void Configure(string? name, OAuthOptions options)
    {
        if (string.IsNullOrEmpty(name)) return;

        using var scope = scopeFactory.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IIdentityProviderRepository>();
        var provider = repo.GetProviderByAliasAsync(name).GetAwaiter().GetResult();

        if (provider is null || !provider.IsEnabled) return;

        var props = MergeProperties(provider);

        options.CallbackPath = "/connect/external/" + name + "/finalize";
        options.SignInScheme = SsoDefaults.ExternalCookieScheme;
        options.SaveTokens = false;

        options.ClientId = props.GetValueOrDefault("client_id", "disabled");
        options.ClientSecret = props.GetValueOrDefault("client_secret", "disabled");
        options.AuthorizationEndpoint = props.GetValueOrDefault("authorization_endpoint", "");
        options.TokenEndpoint = props.GetValueOrDefault("token_endpoint", "");
        options.UserInformationEndpoint = props.GetValueOrDefault("userinfo_endpoint", "");

        var defaultScopes = props.GetValueOrDefault("default_scopes", "");
        foreach (var scopeValue in defaultScopes.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            options.Scope.Add(scopeValue);

        var idField = props.GetValueOrDefault("id_field", "sub");
        var nameField = props.GetValueOrDefault("name_field", "name");
        var emailField = props.GetValueOrDefault("email_field", "email");

        options.ClaimActions.Clear();
        options.ClaimActions.Add(new JsonKeyClaimAction(ClaimTypes.NameIdentifier, ClaimValueTypes.String, idField));
        options.ClaimActions.Add(new JsonKeyClaimAction(ClaimTypes.Name, ClaimValueTypes.String, nameField));
        options.ClaimActions.Add(new JsonKeyClaimAction(ClaimTypes.Email, ClaimValueTypes.String, emailField));

        options.Events = new OAuthEvents
        {
            OnCreatingTicket = async ctx =>
            {
                using var req = new HttpRequestMessage(HttpMethod.Get, ctx.Options.UserInformationEndpoint);
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ctx.AccessToken);
                req.Headers.Add("User-Agent", "Mothenticate");
                req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                using var resp = await ctx.Backchannel.SendAsync(req, ctx.HttpContext.RequestAborted);
                resp.EnsureSuccessStatusCode();

                var rawJson = await resp.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(rawJson);
                ctx.RunClaimActions(doc.RootElement);
                ctx.Identity!.AddClaim(new Claim(SsoDefaults.RawProfileClaimType, rawJson));
            }
        };
    }

    public void Configure(OAuthOptions options) => Configure(Microsoft.Extensions.Options.Options.DefaultName, options);

    private static Dictionary<string, string> MergeProperties(IdpEntity provider)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (provider.ProviderType.DefaultConfiguration is not null)
        {
            var baseProps = DeserializeProps(provider.ProviderType.DefaultConfiguration.Properties);
            foreach (var kv in baseProps) result[kv.Key] = kv.Value;
        }

        if (provider.Configuration is not null)
        {
            var instanceProps = DeserializeProps(provider.Configuration.Properties);
            foreach (var kv in instanceProps) result[kv.Key] = kv.Value;
        }

        return result;
    }

    private static Dictionary<string, string> DeserializeProps(string json)
    {
        try { return JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? []; }
        catch { return []; }
    }
}
