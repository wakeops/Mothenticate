using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mothenticate;
using Mothenticate.Data;
using Mothenticate.IdentityProvider;
using Mothenticate.UserManagement;

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

// Middleware
app.UseAppApi();

await app.RunAsync();

public partial class Program { }