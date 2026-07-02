using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Mothenticate.Data.Services;

namespace Mothenticate.Middleware;

public class SetupMiddleware(RequestDelegate next)
{
    private static readonly string[] BypassPrefixes =
        ["/setup", "/account/setup", "/_blazor", "/_framework", "/_content", "/connect", "/.well-known"];

    public async Task InvokeAsync(HttpContext context, SetupState state, IServiceScopeFactory scopeFactory)
    {
        if (!state.IsConfigured)
        {
            var path = context.Request.Path.Value ?? "/";

            if (!Path.HasExtension(path) &&
                !BypassPrefixes.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var seed = scope.ServiceProvider.GetRequiredService<ISeedService>();

                if (await seed.IsFirstRunAsync())
                {
                    context.Response.Redirect("/setup");
                    return;
                }

                state.MarkConfigured();
            }
        }

        await next(context);
    }
}
