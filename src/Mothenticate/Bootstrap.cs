using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Mothenticate.Domain;
using Mothenticate.Domain.Config;
using Mothenticate.Middleware;
using MudBlazor.Services;
using Serilog;
using Serilog.Formatting.Compact;

namespace Mothenticate;

public static class Bootstrap
{
    public static AppConfig AddAppConfiguration(this IConfigurationBuilder builder, IHostEnvironment hostEnvironment)
    {
        var environmentName = hostEnvironment.EnvironmentName;

        builder
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{environmentName}.json", true)
            .AddEnvironmentVariables();

        return builder.Build().Get<AppConfig>() ?? new AppConfig();
    }

    public static ILoggingBuilder AddAppLogging(this ILoggingBuilder builder, IHostEnvironment hostEnvironment, IConfiguration configuration)
    {
        builder.ClearProviders();

        var loggerConfig = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", Constants.AppName);

        if (hostEnvironment.IsDevelopment())
        {
            loggerConfig.WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}][{SourceContext}]{NewLine}{Message:lj}{NewLine}{Exception}");
        }
        else
        {
            loggerConfig.WriteTo.Console(new CompactJsonFormatter());
        }

        builder.AddSerilog(loggerConfig.CreateLogger());

        return builder;
    }

    public static IServiceCollection AddAppRateLimiting(this IServiceCollection services)
    {
        return services.AddRateLimiter(options =>
        {
            options.AddPolicy("login", context =>
                RateLimitPartition.GetSlidingWindowLimiter(
                    partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(1),
                        SegmentsPerWindow = 6,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    }));

            options.AddPolicy("oauth", context =>
                RateLimitPartition.GetSlidingWindowLimiter(
                    partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = 60,
                        Window = TimeSpan.FromMinutes(1),
                        SegmentsPerWindow = 6,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    }));

            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        });
    }

    public static IServiceCollection AddAppApi(this IServiceCollection services, AppConfig appConfig)
    {
        services
            .AddMvcCore()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            })
            .AddDataAnnotations()
            .AddAuthorization(options =>
            {
                options.AddPolicy(AuthDefaults.AppAdminUserPolicy,
                    policy => policy.AddAuthenticationSchemes("Bearer", AuthDefaults.AppCookieScheme)
                                    .RequireRole(AuthDefaults.AppAdminUserRole));
            });

        services
            .AddAuthentication(AuthDefaults.AppCookieScheme)
            .AddCookie(AuthDefaults.AppCookieScheme, options =>
            {
                options.SlidingExpiration = false;
                options.ExpireTimeSpan = TimeSpan.FromHours(10);
                options.Cookie = new CookieBuilder
                {
                    Name = Constants.AppName,
                    SecurePolicy = CookieSecurePolicy.SameAsRequest
                };
            })
            .AddJwtBearer("Bearer", options =>
            {
                options.Authority = appConfig.IssuerUri;
                options.Audience = AuthDefaults.AppBearerAudience;
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
            });

        services.AddRazorComponents()
            .AddInteractiveServerComponents();

        services.AddMudServices();
        services.AddCascadingAuthenticationState();

        services
            .AddAntiforgery(options =>
            {
                options.Cookie = new CookieBuilder
                {
                    Name = $"{Constants.AppName}.xsrf",
                    SecurePolicy = CookieSecurePolicy.SameAsRequest
                };
            })
            .AddCors();

        return services;
    }

    public static WebApplication UseAppApi(this WebApplication app)
    {
        var forwardedHeaderOptions = new ForwardedHeadersOptions
        {
            RequireHeaderSymmetry = false,
            ForwardLimit = 15,
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
        };
        forwardedHeaderOptions.KnownIPNetworks.Clear();
        forwardedHeaderOptions.KnownProxies.Clear();

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseStatusCodePages();

        app
            .UseForwardedHeaders(forwardedHeaderOptions)
            .UseMiddleware<SetupMiddleware>();

        app.MapStaticAssets().ShortCircuit();

        app
            .UseRouting()
            .UseRateLimiter()
            .UseAntiforgery()
            .UseAuthentication()
            .UseAuthorization();

        app.MapGet("/", () => Results.Redirect("/portal"));
        app.MapControllers();
        app.MapDefaultControllerRoute();
        app.MapRazorComponents<App>()
           .AddInteractiveServerRenderMode();

        return app;
    }
}
