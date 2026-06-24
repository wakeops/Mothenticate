using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Mothenticate.Data.Repositories;

namespace Mothenticate.IdentityProvider.Sso;

public sealed class GoogleOptionsConfigurator(IServiceScopeFactory scopeFactory)
    : IConfigureNamedOptions<GoogleOptions>
{
    public void Configure(string? name, GoogleOptions options)
    {
        if (name != GoogleDefaults.AuthenticationScheme)
        {
            return;
        }

        using var scope = scopeFactory.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IAppSettingsRepository>();
        var settings = repo.GetAsync().GetAwaiter().GetResult();

        // CallbackPath must match ExternalAuthController's finalize route
        options.CallbackPath = "/connect/external/google/finalize";

        if (!settings.GoogleSsoEnabled)
        {
            options.ClientId = "disabled";
            options.ClientSecret = "disabled";
            return;
        }

        options.ClientId = settings.GoogleClientId ?? string.Empty;
        options.ClientSecret = settings.GoogleClientSecret ?? string.Empty;
        options.SignInScheme = SsoDefaults.ExternalCookieScheme;
        options.SaveTokens = false;

        options.ClaimActions.Clear();
        options.ClaimActions.Add(new Microsoft.AspNetCore.Authentication.OAuth.Claims.JsonKeyClaimAction(ClaimTypes.NameIdentifier, ClaimValueTypes.String, "sub"));
        options.ClaimActions.Add(new Microsoft.AspNetCore.Authentication.OAuth.Claims.JsonKeyClaimAction(ClaimTypes.Name, ClaimValueTypes.String, "name"));
        options.ClaimActions.Add(new Microsoft.AspNetCore.Authentication.OAuth.Claims.JsonKeyClaimAction(ClaimTypes.Email, ClaimValueTypes.String, "email"));
    }

    public void Configure(GoogleOptions options) =>
        Configure(Microsoft.Extensions.Options.Options.DefaultName, options);
}
