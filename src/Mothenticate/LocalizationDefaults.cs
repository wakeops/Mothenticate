namespace Mothenticate;

public static class LocalizationDefaults
{
    public const string DefaultCulture = "en";

    public static readonly IReadOnlyList<(string Code, string DisplayName)> SupportedLanguages =
    [
        ("en", "English"),
    ];
}
