using System.Text.Json;

namespace Mothenticate.IdentityProvider.Services.IdentityProviderMappers.Abstract;

/// <summary>
/// Resolves a (UserAttribute, value) pair out of the external provider's raw profile JSON. Persistence
/// and SyncMode (Import vs. Update) semantics are handled by the caller — this only extracts the value.
/// </summary>
public interface IAttributeImportMapper : IIdentityProviderMapper
{
    (int UserAttributeId, string Value)? Resolve(JsonElement providerProfile, IReadOnlyDictionary<string, string> config);
}
