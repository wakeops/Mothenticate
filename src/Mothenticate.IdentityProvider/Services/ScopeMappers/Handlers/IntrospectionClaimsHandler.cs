using Microsoft.AspNetCore.Identity;
using Mothenticate.Data.Entities;
using OpenIddict.Abstractions;
using OpenIddict.Server;

namespace Mothenticate.IdentityProvider.Services.ScopeMappers.Handlers;

public class IntrospectionClaimsHandler(
    UserManager<ApplicationUser> userManager,
    IScopeMapperClaimsService scopeMapperClaimsService) : IOpenIddictServerHandler<OpenIddictServerEvents.HandleIntrospectionRequestContext>
{
    public async ValueTask HandleAsync(OpenIddictServerEvents.HandleIntrospectionRequestContext context)
    {
        var scopes = context.GenericTokenPrincipal?.GetScopes().ToList() ?? [];
        if (context.Subject is null || scopes.Count == 0)
        {
            return;
        }

        var user = await userManager.FindByIdAsync(context.Subject);
        if (user is null)
        {
            return;
        }

        var claims = await scopeMapperClaimsService.GetIntrospectionClaimsAsync(user, scopes, context.CancellationToken);
        foreach (var (type, value) in claims)
        {
            context.Claims[type] = value;
        }
    }
}
