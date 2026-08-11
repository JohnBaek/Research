using System.Text.Json;
using AwesomeAssertions;
using DotNet_10.Features;

namespace DotNet_10.Tests;

// [Feature09] JsonSerializerOptions.Strict — 엄격 검증 동작 확인
public class JsonStrictOptionsTests
{
    [Fact]
    public void Strict_는_매핑안되는_속성이_있으면_예외()
    {
        var json = """{ "Name": "svc", "Port": 8080, "Unknown": true }""";

        var act = () => JsonStrictDemo.DeserializeStrict(json);

        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void Default_는_매핑안되는_속성을_조용히_무시한다()
    {
        var json = """{ "Name": "svc", "Port": 8080, "Unknown": true }""";

        var config = JsonStrictDemo.DeserializeDefault(json);

        config!.Name.Should().Be("svc");
        config.Port.Should().Be(8080);
    }

    [Fact]
    public void Strict_는_대소문자가_다르면_예외()
    {
        // Strict는 대소문자를 구분 → "name"은 "Name"에 매핑되지 않는다.
        var json = """{ "name": "svc" }""";

        var act = () => JsonStrictDemo.DeserializeStrict(json);

        act.Should().Throw<JsonException>();
    }
}
