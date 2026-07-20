using System.Text.Json;
using Mothenticate.Data.Entities;
using Mothenticate.IdentityProvider.Services.IdentityProviderMappers.Abstract;
using Mothenticate.IdentityProvider.Services.IdentityProviderMappers.MapperConfigs;
using Mothenticate.IdentityProvider.Services.ScopeMappers.Abstract;

namespace Mothenticate.IdentityProvider.Services.IdentityProviderMappers;

public class AttributeImporterMapper : ConfigurableMapper<AttributeImporterMapperConfig>, IAttributeImportMapper
{
    public IdentityProviderMapperType MapperType => IdentityProviderMapperType.AttributeImporter;

    public (int UserAttributeId, string Value)? Resolve(JsonElement providerProfile, IReadOnlyDictionary<string, string> config)
    {
        var settings = GetSettingsFromConfig(config);
        if (settings.UserAttributeId is null || string.IsNullOrWhiteSpace(settings.ProviderFieldPath))
        {
            return null;
        }

        var value = ResolveFieldPath(providerProfile, settings.ProviderFieldPath);
        return value is null ? null : (settings.UserAttributeId.Value, value);
    }

    // Simple dot-notation traversal through nested JSON objects (e.g. "address.country") — not full
    // JSONPath, just enough to reach fields the provider's userinfo response commonly nests.
    private static string? ResolveFieldPath(JsonElement root, string path)
    {
        var current = root;
        foreach (var segment in path.Split('.', StringSplitOptions.RemoveEmptyEntries))
        {
            if (current.ValueKind != JsonValueKind.Object || !current.TryGetProperty(segment, out var next))
            {
                return null;
            }
            current = next;
        }

        return current.ValueKind switch
        {
            JsonValueKind.String => current.GetString(),
            JsonValueKind.Number => current.GetRawText(),
            JsonValueKind.True or JsonValueKind.False => current.GetBoolean().ToString().ToLowerInvariant(),
            _ => null
        };
    }
}
