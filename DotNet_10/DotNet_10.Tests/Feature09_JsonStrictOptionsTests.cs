using System.Text.Json;
using AwesomeAssertions;
using DotNet_10.Features;

namespace DotNet_10.Tests;

// [Feature09] JsonSerializerOptions.Strict - check the strict validation behavior
public class JsonStrictOptionsTests
{
    [Fact]
    public void Strict_throws_when_a_property_does_not_map()
    {
        var json = """{ "Name": "svc", "Port": 8080, "Unknown": true }""";

        var act = () => JsonStrictDemo.DeserializeStrict(json);

        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void Default_silently_ignores_an_unmapped_property()
    {
        var json = """{ "Name": "svc", "Port": 8080, "Unknown": true }""";

        var config = JsonStrictDemo.DeserializeDefault(json);

        config!.Name.Should().Be("svc");
        config.Port.Should().Be(8080);
    }

    [Fact]
    public void Strict_throws_when_the_case_does_not_match()
    {
        // Strict is case-sensitive -> "name" does not map to "Name".
        var json = """{ "name": "svc" }""";

        var act = () => JsonStrictDemo.DeserializeStrict(json);

        act.Should().Throw<JsonException>();
    }
}
