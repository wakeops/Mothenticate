using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Mothenticate.Data;
using Mothenticate.Data.Entities;
using Mothenticate.Domain.Config;
using Mothenticate.IdentityProvider.Services;
using Mothenticate.IdentityProvider.Services.IdentityProviderMappers;
using Mothenticate.IdentityProvider.Services.IdentityProviderMappers.Abstract;
using Mothenticate.IdentityProvider.Services.ScopeMappers;
using Mothenticate.IdentityProvider.Services.ScopeMappers.Abstract;
using Mothenticate.IdentityProvider.Services.ScopeMappers.Handlers;
using Mothenticate.IdentityProvider.Sso;
using OpenIddict.Abstractions;
using OpenIddict.Server;

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
                    OpenIddictConstants.Scopes.OpenId,
                    "acr",
                    "basic");

                options.AddEventHandler<OpenIddictServerEvents.HandleIntrospectionRequestContext>(builder =>
                    builder.UseScopedHandler<IntrospectionClaimsHandler>()
                    .SetOrder(int.MaxValue - 100_000)
                    .SetType(OpenIddictServerHandlerType.Custom));

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
        services.AddScoped<IIdentityProviderService, IdentityProviderService>();

        services.AddSingleton<IScopeMapper, UserAttributeMapper>();
        services.AddSingleton<IScopeMapper, UserPropertyMapper>();
        services.AddSingleton<IScopeMapper, AcrMapper>();
        services.AddSingleton<IScopeMapper, SubjectMapper>();
        services.AddSingleton<IScopeMapperResolver, ScopeMapperResolver>();
        services.AddScoped<IntrospectionClaimsHandler>();

        services.AddSingleton<IIdentityProviderMapper, AttributeImporterMapper>();
        services.AddSingleton<IIdentityProviderMapperResolver, IdentityProviderMapperResolver>();

        // Dynamic SSO — schemes and options are loaded from DB at runtime, no restart needed
        var changeSource = new SsoOptionsChangeSource();
        services.AddSingleton(changeSource);
        services.AddSingleton<IOptionsChangeTokenSource<OAuthOptions>>(changeSource);
        services.AddSingleton<ISsoSettingsChangeNotifier>(changeSource);
        services.AddSingleton<IConfigureOptions<OAuthOptions>, DynamicOAuthOptionsConfigurator>();
        services.AddSingleton<IAuthenticationSchemeProvider, DynamicAuthSchemeProvider>();

        // Register OAuthHandler so the DI container can resolve it for dynamic schemes
        services.AddTransient<OAuthHandler<OAuthOptions>>();
        services.AddSingleton<IPostConfigureOptions<OAuthOptions>, OAuthPostConfigureOptions<OAuthOptions, OAuthHandler<OAuthOptions>>>();

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
            });

        return services;
    }
}
