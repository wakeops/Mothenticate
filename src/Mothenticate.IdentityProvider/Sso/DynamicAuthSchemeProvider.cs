using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Mothenticate.Data.Repositories;

namespace Mothenticate.IdentityProvider.Sso;

public class DynamicAuthSchemeProvider(
    IOptions<AuthenticationOptions> options,
    IServiceScopeFactory scopeFactory)
    : AuthenticationSchemeProvider(options)
{
    public override async Task<AuthenticationScheme?> GetSchemeAsync(string name)
    {
        var scheme = await base.GetSchemeAsync(name);
        if (scheme is not null) return scheme;

        return await FindDynamicSchemeAsync(name);
    }

    public override async Task<IEnumerable<AuthenticationScheme>> GetRequestHandlerSchemesAsync()
    {
        var staticSchemes = (await base.GetRequestHandlerSchemesAsync()).ToList();
        var dynamicSchemes = await LoadAllDynamicSchemesAsync();
        return staticSchemes.Concat(dynamicSchemes);
    }

    public override async Task<IEnumerable<AuthenticationScheme>> GetAllSchemesAsync()
    {
        var staticSchemes = (await base.GetAllSchemesAsync()).ToList();
        var dynamicSchemes = await LoadAllDynamicSchemesAsync();
        return staticSchemes.Concat(dynamicSchemes);
    }

    private async Task<AuthenticationScheme?> FindDynamicSchemeAsync(string alias)
    {
        using var scope = scopeFactory.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IIdentityProviderRepository>();
        var provider = await repo.GetProviderByAliasAsync(alias);
        if (provider is null || !provider.IsEnabled) return null;
        return new AuthenticationScheme(provider.Alias, provider.DisplayName, typeof(OAuthHandler<OAuthOptions>));
    }

    private async Task<IEnumerable<AuthenticationScheme>> LoadAllDynamicSchemesAsync()
    {
        using var scope = scopeFactory.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IIdentityProviderRepository>();
        var providers = await repo.GetProvidersAsync();
        return providers
            .Where(p => p.IsEnabled)
            .Select(p => new AuthenticationScheme(p.Alias, p.DisplayName, typeof(OAuthHandler<OAuthOptions>)));
    }
}
