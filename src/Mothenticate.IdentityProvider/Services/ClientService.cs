using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Mothenticate.Data.Entities;
using Mothenticate.Data.Repositories;
using Mothenticate.IdentityProvider.Models;
using OpenIddict.Abstractions;

namespace Mothenticate.IdentityProvider.Services;

public class ClientService(
    IOpenIddictApplicationManager applicationManager,
    IClientSecretRepository clientSecretRepository,
    IPasswordHasher<ClientSecret> passwordHasher) : IClientService
{
    private const string _isEnabledKey = "mothenticate:is_enabled";

    public async Task<IReadOnlyList<ClientInfo>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var results = new List<ClientInfo>();
        await foreach (var app in applicationManager.ListAsync(cancellationToken: cancellationToken))
        {
            results.Add(await MapAsync(app, cancellationToken));
        }

        return results;
    }

    public async Task<ClientInfo?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var app = await applicationManager.FindByIdAsync(id, cancellationToken);
        return app is null ? null : await MapAsync(app, cancellationToken);
    }

    public async Task<ClientInfo?> GetByClientIdAsync(string clientId, CancellationToken cancellationToken = default)
    {
        var app = await applicationManager.FindByClientIdAsync(clientId, cancellationToken);
        return app is null ? null : await MapAsync(app, cancellationToken);
    }

    public async Task<string?> CreateAsync(OpenIddictApplicationDescriptor descriptor, CancellationToken cancellationToken = default)
    {
        // Default new clients to enabled
        descriptor.Properties.TryAdd(_isEnabledKey, JsonSerializer.SerializeToElement(true));
        var app = await applicationManager.CreateAsync(descriptor, cancellationToken);
        return await applicationManager.GetIdAsync(app, cancellationToken);
    }

    public async Task UpdateAsync(string id, OpenIddictApplicationDescriptor descriptor, CancellationToken cancellationToken = default)
    {
        var app = await applicationManager.FindByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException($"Client '{id}' not found.");

        // Preserve IsEnabled when the caller doesn't explicitly include it
        if (!descriptor.Properties.ContainsKey(_isEnabledKey))
        {
            var current = await applicationManager.GetPropertiesAsync(app, cancellationToken);
            if (current.TryGetValue(_isEnabledKey, out var existing))
            {
                descriptor.Properties[_isEnabledKey] = existing;
            }
        }

        await applicationManager.UpdateAsync(app, descriptor, cancellationToken);
    }

    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var app = await applicationManager.FindByIdAsync(id, cancellationToken);
        if (app is not null)
        {
            await applicationManager.DeleteAsync(app, cancellationToken);
        }
    }

    public async Task SetEnabledAsync(string applicationId, bool isEnabled, CancellationToken cancellationToken = default)
    {
        var app = await applicationManager.FindByIdAsync(applicationId, cancellationToken)
            ?? throw new InvalidOperationException($"Client '{applicationId}' not found.");

        var descriptor = new OpenIddictApplicationDescriptor();
        await applicationManager.PopulateAsync(descriptor, app, cancellationToken);
        descriptor.Properties[_isEnabledKey] = JsonSerializer.SerializeToElement(isEnabled);

        await applicationManager.UpdateAsync(app, descriptor, cancellationToken);
    }

    public async Task<IReadOnlyList<ClientSecretInfo>> GetSecretsAsync(string applicationId, CancellationToken cancellationToken = default)
    {
        var secrets = await clientSecretRepository.GetByApplicationIdAsync(applicationId, cancellationToken);
        return secrets.Select(s => new ClientSecretInfo
        {
            Id = s.Id,
            Description = s.Description,
            CreatedAt = s.CreatedAt,
            ExpiresAt = s.ExpiresAt
        }).ToList();
    }

    public async Task<int> AddSecretAsync(string applicationId, string secretValue, string? description = null, DateTimeOffset? expiresAt = null, CancellationToken cancellationToken = default)
    {
        var placeholder = new ClientSecret { ApplicationId = applicationId, SecretHash = string.Empty };
        var hash = passwordHasher.HashPassword(placeholder, secretValue);

        var secret = await clientSecretRepository.CreateAsync(new ClientSecret
        {
            ApplicationId = applicationId,
            SecretHash = hash,
            Description = description,
            ExpiresAt = expiresAt
        }, cancellationToken);

        return secret.Id;
    }

    public Task RemoveSecretAsync(int secretId, CancellationToken cancellationToken = default)
        => clientSecretRepository.DeleteAsync(secretId, cancellationToken);

    public async Task<bool> ValidateSecretAsync(string applicationId, string secretValue, CancellationToken cancellationToken = default)
    {
        var secrets = await clientSecretRepository.GetByApplicationIdAsync(applicationId, cancellationToken);
        var activeSecrets = secrets.Where(s => s.ExpiresAt == null || s.ExpiresAt > DateTimeOffset.UtcNow);

        foreach (var secret in activeSecrets)
        {
            var placeholder = new ClientSecret { ApplicationId = applicationId, SecretHash = secret.SecretHash };
            var result = passwordHasher.VerifyHashedPassword(placeholder, secret.SecretHash, secretValue);
            if (result != PasswordVerificationResult.Failed)
            {
                return true;
            }
        }

        return false;
    }

    private async Task<ClientInfo> MapAsync(object app, CancellationToken cancellationToken)
    {
        var id = await applicationManager.GetIdAsync(app, cancellationToken);
        var redirectUris = await applicationManager.GetRedirectUrisAsync(app, cancellationToken);
        var postLogoutUris = await applicationManager.GetPostLogoutRedirectUrisAsync(app, cancellationToken);
        var permissions = await applicationManager.GetPermissionsAsync(app, cancellationToken);
        var properties = await applicationManager.GetPropertiesAsync(app, cancellationToken);
        var settings = await applicationManager.GetSettingsAsync(app, cancellationToken);
        var isEnabled = !properties.TryGetValue(_isEnabledKey, out var val) || val.GetBoolean();

        static int? ReadSeconds(System.Collections.Immutable.ImmutableDictionary<string, string> s, string key)
            => s.TryGetValue(key, out var v) && TimeSpan.TryParse(v, out var ts) ? (int)ts.TotalSeconds : null;

        return new ClientInfo
        {
            Id = id,
            ClientId = await applicationManager.GetClientIdAsync(app, cancellationToken),
            DisplayName = await applicationManager.GetDisplayNameAsync(app, cancellationToken),
            ClientType = await applicationManager.GetClientTypeAsync(app, cancellationToken),
            RedirectUris = [.. redirectUris],
            PostLogoutRedirectUris = [.. postLogoutUris],
            Permissions = [.. permissions],
            IsEnabled = isEnabled,
            AccessTokenLifetime = ReadSeconds(settings, OpenIddictConstants.Settings.TokenLifetimes.AccessToken),
            IdentityTokenLifetime = ReadSeconds(settings, OpenIddictConstants.Settings.TokenLifetimes.IdentityToken),
            RefreshTokenLifetime = ReadSeconds(settings, OpenIddictConstants.Settings.TokenLifetimes.RefreshToken),
        };
    }
}
