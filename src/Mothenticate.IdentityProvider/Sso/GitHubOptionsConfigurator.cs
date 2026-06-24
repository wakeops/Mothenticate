using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Mothenticate.Data.Repositories;

namespace Mothenticate.IdentityProvider.Sso;

public sealed class GitHubOptionsConfigurator(IServiceScopeFactory scopeFactory)
    : IConfigureNamedOptions<OAuthOptions>
{
    public void Configure(string? name, OAuthOptions options)
    {
        if (name != SsoDefaults.GitHubScheme)
        {
            return;
        }

        using var scope = scopeFactory.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IAppSettingsRepository>();
        var settings = repo.GetAsync().GetAwaiter().GetResult();

        // CallbackPath must always be set — OAuthHandler is a RemoteAuthenticationHandler
        // and is initialized on every request as an IAuthenticationRequestHandler.
        options.CallbackPath = "/connect/external/github/finalize";
        options.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
        options.TokenEndpoint = "https://github.com/login/oauth/access_token";
        options.UserInformationEndpoint = "https://api.github.com/user";

        if (!settings.GitHubSsoEnabled)
        {
            options.ClientId = "disabled";
            options.ClientSecret = "disabled";
            return;
        }

        options.ClientId = settings.GitHubClientId ?? string.Empty;
        options.ClientSecret = settings.GitHubClientSecret ?? string.Empty;
        options.SignInScheme = SsoDefaults.ExternalCookieScheme;
        options.SaveTokens = false;
        options.Scope.Add("user:email");

        options.ClaimActions.Clear();
        options.ClaimActions.Add(new Microsoft.AspNetCore.Authentication.OAuth.Claims.JsonKeyClaimAction(ClaimTypes.NameIdentifier, ClaimValueTypes.String, "id"));
        options.ClaimActions.Add(new Microsoft.AspNetCore.Authentication.OAuth.Claims.JsonKeyClaimAction(ClaimTypes.Name, ClaimValueTypes.String, "login"));
        options.ClaimActions.Add(new Microsoft.AspNetCore.Authentication.OAuth.Claims.JsonKeyClaimAction(ClaimTypes.Email, ClaimValueTypes.String, "email"));

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

                using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
                ctx.RunClaimActions(doc.RootElement);
            }
        };
    }

    public void Configure(OAuthOptions options) =>
        Configure(SsoDefaults.GitHubScheme, options);
}
