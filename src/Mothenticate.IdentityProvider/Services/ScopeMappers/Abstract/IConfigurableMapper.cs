namespace Mothenticate.IdentityProvider.Services.ScopeMappers.Abstract;

/// <summary>
/// Non-generic marker implemented by every <see cref="IConfigurableMapper{T}"/> — lets callers (e.g. the
/// admin UI) discover a mapper's settings model type via <see cref="SettingsType"/> without knowing T ahead
/// of time, so config fields can be rendered dynamically instead of hardcoded per mapper type.
/// </summary>
public interface IConfigurableMapper
{
    Type SettingsType { get; }
}

public interface IConfigurableMapper<T> : IConfigurableMapper where T : class
{
    T GetSettingsFromConfig(IReadOnlyDictionary<string, string> config);
}
