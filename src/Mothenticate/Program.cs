using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mothenticate;
using Mothenticate.Data;
using Mothenticate.IdentityProvider;
using Mothenticate.UserManagement;
using Mothenticate.IdentityProvider.Services;
using Mothenticate.UserManagement.Services;

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

var builder = WebApplication.CreateBuilder(args);

// Configuration
var appConfig = builder.Configuration.AddAppConfiguration(builder.Environment);

// Logging
builder.Logging.AddAppLogging(builder.Environment, builder.Configuration);

// Services
builder.Services.AddSingleton(appConfig);

builder.Services.AddMemoryCache();
builder.Services.AddHttpClient();

builder.Services.AddAppRateLimiting();
builder.Services.AddAppData(appConfig.PostgreSQL);
builder.Services.AddUserManagement();
builder.Services.AddIdentityProvider(appConfig, builder.Environment);

builder.Services.AddAppApi(appConfig);

// Build
var app = builder.Build();

// Migrations and seed
await app.Services.RunAppDataMigrationsAsync();
await using (var scope = app.Services.CreateAsyncScope())
{
    var idpService = scope.ServiceProvider.GetRequiredService<IIdentityProviderService>();
    await idpService.SeedProviderTypesAsync();
}

// Middleware
app.UseAppApi();

await app.RunAsync();

public partial class Program { }