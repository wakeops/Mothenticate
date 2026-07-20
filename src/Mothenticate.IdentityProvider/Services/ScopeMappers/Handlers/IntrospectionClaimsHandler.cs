using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Mothenticate.Data.Entities;
using Mothenticate.IdentityProvider.Services.ScopeMappers.Abstract;
using Mothenticate.UserManagement.Services;
using OpenIddict.Abstractions;
using OpenIddict.Server;

namespace Mothenticate.IdentityProvider.Services.ScopeMappers.Handlers;

public class IntrospectionClaimsHandler(
    UserManager<ApplicationUser> userManager,
    IUserAttributeService userAttributeService,
    IClientScopeService clientScopeService,
    IScopeMapperResolver scopeMapperResolver) : IOpenIddictServerHandler<OpenIddictServerEvents.HandleIntrospectionRequestContext>
{
    public async ValueTask HandleAsync(OpenIddictServerEvents.HandleIntrospectionRequestContext context)
    {
        var scopes = context.GenericTokenPrincipal?.GetScopes().ToList() ?? [];
        if (context.Subject is null || scopes.Count == 0)
        {
            return;
        }

        var mapperRows = await clientScopeService.GetMappersByScopeNamesAsync(scopes);
        if (mapperRows.Count == 0)
        {
            return;
        }

        var user = await userManager.FindByIdAsync(context.Subject);
        if (user is null)
        {
            return;
        }

        var userAttributes = await userAttributeService.GetAllWithUserValuesAsync(user.Id);
        var identity = new ClaimsIdentity();

        foreach (var mapperRow in mapperRows)
        {
            if (scopeMapperResolver.Resolve(mapperRow.MapperType) is not IIntrospectionTokenMapper mapper)
            {
                continue;
            }

            var config = DeserializeConfig(mapperRow.Config);
            if (!GetConfigFlag(config, "IncludeIntrospectionToken"))
            {
                continue;
            }

            var claims = await mapper.MapTokenAsync(identity, user, userAttributes, config, context.CancellationToken);
            foreach (var claim in claims)
            {
                context.Claims[claim.Type] = claim.Value;
            }
        }
    }

    private static bool GetConfigFlag(IReadOnlyDictionary<string, string> config, string key)
        => config.TryGetValue(key, out var value) && bool.TryParse(value, out var parsed) && parsed;

    private static Dictionary<string, string> DeserializeConfig(string json)
        => JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? [];
}
