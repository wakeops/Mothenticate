using System.Text.Json;

namespace Mothenticate.IdentityProvider.Services.ScopeMappers.Abstract;

/// <summary>
/// Single place that (de)serializes a mapper's <c>Config</c> column — a flat Dictionary&lt;string,string&gt;
/// stored as JSON — so every caller (controllers, OpenIddict handlers, admin Razor pages) shares the same
/// behavior instead of each hand-rolling its own JsonSerializer call.
/// </summary>
public static class MapperConfigSerializer
{
    public static Dictionary<string, string> Deserialize(string json)
        => JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? [];

    public static string Serialize(IReadOnlyDictionary<string, string> config)
        => JsonSerializer.Serialize(config);
}
