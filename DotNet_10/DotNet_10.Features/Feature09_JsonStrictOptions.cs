using System.Text.Json;

namespace DotNet_10.Features;

/// <summary>
/// [.NET 10 BCL] JsonSerializerOptions.Strict preset.
///
/// System.Text.Json is "lenient" by default - if the JSON has extra
/// properties, it silently ignores them. That makes typos or schema
/// mismatches easy to miss.
///
/// .NET 10 adds the JsonSerializerOptions.Strict preset for strict validation:
///   - Throws if a JSON property does not map to the target type
///   - Matches property names case-sensitively (Web preset ignores case)
///   - Rejects duplicate properties, etc.
///
/// -> Good where "silent ignore" is dangerous, like config files or external contracts.
/// </summary>
public static class JsonStrictDemo
{
    /// <summary>Model to deserialize into.</summary>
    public sealed class AppConfig
    {
        public string? Name { get; set; }
        public int Port { get; set; }
    }

    /// <summary>Deserialize with the Strict preset. Throws JsonException if it does not fit the schema.</summary>
    public static AppConfig? DeserializeStrict(string json) =>
        JsonSerializer.Deserialize<AppConfig>(json, JsonSerializerOptions.Strict);

    /// <summary>Deserialize with default options (control). Extra properties are silently ignored.</summary>
    public static AppConfig? DeserializeDefault(string json) =>
        JsonSerializer.Deserialize<AppConfig>(json);
}
