using Mothenticate.IdentityProvider.Models;
using OpenIddict.Abstractions;

namespace Mothenticate.IdentityProvider.Services;

public class ConsentService(
    IOpenIddictAuthorizationManager authorizationManager,
    IOpenIddictApplicationManager applicationManager) : IConsentService
{
    public async Task<IReadOnlyList<ConsentInfo>> GetBySubjectAsync(string userId, CancellationToken cancellationToken = default)
    {
        var results = new List<ConsentInfo>();

        await foreach (var authorization in authorizationManager.FindBySubjectAsync(userId, cancellationToken))
        {
            var status = await authorizationManager.GetStatusAsync(authorization, cancellationToken);
            if (status is not OpenIddictConstants.Statuses.Valid)
            {
                continue;
            }

            var appId = await authorizationManager.GetApplicationIdAsync(authorization, cancellationToken);
            string? appName = null;
            if (appId is not null)
            {
                var app = await applicationManager.FindByIdAsync(appId, cancellationToken);
                if (app is not null)
                {
                    appName = await applicationManager.GetDisplayNameAsync(app, cancellationToken);
                }
            }

            var scopes = await authorizationManager.GetScopesAsync(authorization, cancellationToken);

            results.Add(new ConsentInfo
            {
                Id = await authorizationManager.GetIdAsync(authorization, cancellationToken),
                Subject = await authorizationManager.GetSubjectAsync(authorization, cancellationToken),
                ApplicationId = appId,
                ApplicationName = appName,
                Status = status,
                Scopes = [.. scopes],
                CreationDate = await authorizationManager.GetCreationDateAsync(authorization, cancellationToken)
            });
        }

        return results;
    }

    public async Task RevokeAsync(string authorizationId, CancellationToken cancellationToken = default)
    {
        var authorization = await authorizationManager.FindByIdAsync(authorizationId, cancellationToken);
        if (authorization is not null)
        {
            await authorizationManager.DeleteAsync(authorization, cancellationToken);
        }
    }

    public async Task RevokeAllBySubjectAsync(string userId, CancellationToken cancellationToken = default)
    {
        var toDelete = new List<object>();
        await foreach (var authorization in authorizationManager.FindBySubjectAsync(userId, cancellationToken))
        {
            toDelete.Add(authorization);
        }

        foreach (var authorization in toDelete)
        {
            await authorizationManager.DeleteAsync(authorization, cancellationToken);
        }
    }
}
