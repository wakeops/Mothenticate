using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mothenticate.IdentityProvider.Services.ScopeMappers.Abstract;

public abstract class ConfigurableMapper<TSettingsModel> : IConfigurableMapper<TSettingsModel> where TSettingsModel : class
{
    // Config is always a flat Dictionary<string,string> (every value is a JSON string), but settings
    // models may declare numeric/bool properties (e.g. int? UserAttributeId) — AllowReadingFromString
    // lets those round-trip without every mapper having to hand-parse strings itself.
    private static readonly JsonSerializerOptions SettingsOptions = new()
    {
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        Converters = { new JsonStringEnumConverter() }
    };

    public Type SettingsType => typeof(TSettingsModel);

    public TSettingsModel GetSettingsFromConfig(IReadOnlyDictionary<string, string> config)
    {
        var serializedConfig = JsonSerializer.Serialize(config);
        return JsonSerializer.Deserialize<TSettingsModel>(serializedConfig, SettingsOptions)
            ?? throw new InvalidOperationException($"Failed to deserialize config to {typeof(TSettingsModel).Name}");
    }
}