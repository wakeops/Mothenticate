using System.Text.Json;
using Mothenticate.Data.Entities;
using Mothenticate.IdentityProvider.Services.IdentityProviderMappers.Abstract;
using Mothenticate.UserManagement.Services;
using IdpEntity = Mothenticate.Data.Entities.IdentityProvider;

namespace Mothenticate.IdentityProvider.Services.IdentityProviderMappers.Handlers;

/// <summary>
/// Applies an IdentityProvider's AttributeImporter mappers against the external provider's raw profile
/// JSON on login. "Import" mode only sets an attribute the first time it has no value for the user;
/// "Update" always overwrites it. This covers both first sign-in and a provider linked later to an
/// existing account, since it keys off whether the attribute currently has a value rather than whether
/// the account was just created.
/// </summary>
public static class AttributeImportHandler
{
    public static async Task ApplyAsync(
        IdpEntity provider,
        string userId,
        JsonElement providerProfile,
        IIdentityProviderMapperResolver resolver,
        IUserAttributeService userAttributeService,
        CancellationToken cancellationToken)
    {
        if (provider.Mappers.Count == 0)
        {
            return;
        }

        var existingValues = await userAttributeService.GetUserValuesAsync(userId, cancellationToken);
        var attributeIdsWithValues = existingValues.Select(v => v.UserAttributeId).ToHashSet();

        foreach (var mapperRow in provider.Mappers)
        {
            if (resolver.Resolve(mapperRow.MapperType) is not IAttributeImportMapper mapper)
            {
                continue;
            }

            var config = DeserializeConfig(mapperRow.Config);
            var resolved = mapper.Resolve(providerProfile, config);
            if (resolved is null)
            {
                continue;
            }

            var (userAttributeId, value) = resolved.Value;

            var effectiveSyncMode = mapperRow.SyncMode == SyncMode.Inherit ? provider.SyncMode : mapperRow.SyncMode;
            if (effectiveSyncMode == SyncMode.Import && attributeIdsWithValues.Contains(userAttributeId))
            {
                continue;
            }

            await userAttributeService.SetUserValuesAsync(userId, userAttributeId, [value], cancellationToken);
        }
    }

    private static Dictionary<string, string> DeserializeConfig(string json)
        => JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? [];
}
