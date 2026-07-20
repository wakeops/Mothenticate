using System.Text.Json;
using Mothenticate.IdentityProvider.Services.IdentityProviderMappers;

namespace Mothenticate.Tests;

public class AttributeImporterMapperTests
{
    private readonly AttributeImporterMapper _mapper = new();

    private static Dictionary<string, string> Config(string fieldPath, int userAttributeId) => new()
    {
        ["ProviderFieldPath"] = fieldPath,
        ["UserAttributeId"] = userAttributeId.ToString()
    };

    [Fact]
    public void Resolve_ReturnsValue_ForTopLevelStringField()
    {
        var profile = JsonDocument.Parse("""{"given_name":"Ada"}""").RootElement;

        var result = _mapper.Resolve(profile, Config("given_name", 5));

        Assert.Equal((5, "Ada"), result);
    }

    [Fact]
    public void Resolve_ReturnsValue_ForNestedDotPathField()
    {
        var profile = JsonDocument.Parse("""{"address":{"country":"UK"}}""").RootElement;

        var result = _mapper.Resolve(profile, Config("address.country", 7));

        Assert.Equal((7, "UK"), result);
    }

    [Fact]
    public void Resolve_ReturnsNull_WhenFieldPathMissing()
    {
        var profile = JsonDocument.Parse("""{"given_name":"Ada"}""").RootElement;

        var result = _mapper.Resolve(profile, Config("family_name", 5));

        Assert.Null(result);
    }

    [Fact]
    public void Resolve_ReturnsNull_WhenUserAttributeIdMissing()
    {
        var profile = JsonDocument.Parse("""{"given_name":"Ada"}""").RootElement;
        var config = new Dictionary<string, string> { ["ProviderFieldPath"] = "given_name" };

        var result = _mapper.Resolve(profile, config);

        Assert.Null(result);
    }

    [Fact]
    public void Resolve_LowercasesBooleanValues()
    {
        var profile = JsonDocument.Parse("""{"email_verified":true}""").RootElement;

        var result = _mapper.Resolve(profile, Config("email_verified", 9));

        Assert.Equal((9, "true"), result);
    }
}
