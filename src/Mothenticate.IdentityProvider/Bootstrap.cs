using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Mothenticate.Data;
using Mothenticate.Data.Entities;
using Mothenticate.Domain.Config;
using Mothenticate.IdentityProvider.Services;
using Mothenticate.IdentityProvider.Sso;
using OpenIddict.Abstractions;

namespace Mothenticate.IdentityProvider;

public static class Bootstrap
{
    public static IServiceCollection AddIdentityProvider(
        this IServiceCollection services,
        AppConfig appConfig,
        IHostEnvironment environment)
    {
        services.AddOpenIddict()
            .AddCore(options =>
                options.UseEntityFrameworkCore()
                       .UseDbContext<MothenticateDbContext>())
            .AddServer(options =>
            {
                if (appConfig.IssuerUri is not null)
                {
                    options.SetIssuer(new Uri(appConfig.IssuerUri));
                }

                options
                    .SetAuthorizationEndpointUris("/connect/authorize")
                    .SetDeviceAuthorizationEndpointUris("/connect/device")
                    .SetEndUserVerificationEndpointUris("/connect/verify")
                    .SetTokenEndpointUris("/connect/token")
                    .SetIntrospectionEndpointUris("/connect/introspect")
                    .SetRevocationEndpointUris("/connect/revoke")
                    .SetUserInfoEndpointUris("/connect/userinfo")
                    .SetEndSessionEndpointUris("/connect/endsession");

                options
                    .AllowAuthorizationCodeFlow()
                    .AllowImplicitFlow()
                    .AllowClientCredentialsFlow()
                    .AllowPasswordFlow()
                    .AllowRefreshTokenFlow()
                    .AllowDeviceAuthorizationFlow();

                options.RegisterScopes(
                    OpenIddictConstants.Scopes.Email,
                    OpenIddictConstants.Scopes.Profile,
                    OpenIddictConstants.Scopes.Roles,
                    OpenIddictConstants.Scopes.OfflineAccess,
                    OpenIddictConstants.Scopes.OpenId);

                if (environment.IsDevelopment())
                {
                    options.AddDevelopmentEncryptionCertificate()
                           .AddDevelopmentSigningCertificate();
                }
                else
                {
                    // TODO: load signing/encryption certificates from configuration for production
                    options.AddDevelopmentEncryptionCertificate()
                           .AddDevelopmentSigningCertificate();
                }

                var aspNetCore = options.UseAspNetCore()
                    .EnableAuthorizationEndpointPassthrough()
                    .EnableTokenEndpointPassthrough()
                    .EnableUserInfoEndpointPassthrough()
                    .EnableEndSessionEndpointPassthrough()
                    .EnableEndUserVerificationEndpointPassthrough()
                    .EnableStatusCodePagesIntegration();

                if (!environment.IsProduction())
                {
                    aspNetCore.DisableTransportSecurityRequirement();
                }
            })
            .AddValidation(options =>
            {
                options.UseLocalServer();
                options.UseAspNetCore();
            });

        services.AddSingleton<IPasswordHasher<ClientSecret>, PasswordHasher<ClientSecret>>();

        services.AddScoped<IClientService, ClientService>();
        services.AddScoped<IConsentService, ConsentService>();
        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<ITwoFactorService, TwoFactorService>();
        services.AddScoped<ISsoService, SsoService>();

        // SSO dynamic options — change source allows cache invalidation when settings are saved
        var changeSource = new SsoOptionsChangeSource();
        services.AddSingleton(changeSource);
        services.AddSingleton<IOptionsChangeTokenSource<GoogleOptions>>(changeSource);
        services.AddSingleton<IOptionsChangeTokenSource<OAuthOptions>>(changeSource);
        services.AddSingleton<IConfigureOptions<GoogleOptions>, GoogleOptionsConfigurator>();
        services.AddSingleton<IConfigureOptions<OAuthOptions>, GitHubOptionsConfigurator>();

        services.AddAuthentication()
            .AddCookie(SsoDefaults.ExternalCookieScheme, o =>
            {
                o.Cookie.Name = "Mothenticate.External";
                o.ExpireTimeSpan = TimeSpan.FromMinutes(10);
            })
            .AddCookie(SsoDefaults.PreAuthCookieScheme, o =>
            {
                o.Cookie.Name = "Mothenticate.PreAuth";
                o.ExpireTimeSpan = TimeSpan.FromMinutes(5);
            })
            .AddGoogle()
            .AddOAuth(SsoDefaults.GitHubScheme, _ => { });

        return services;
    }
}
