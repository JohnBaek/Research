using AwesomeAssertions;
using DotNet_10.Features;

namespace DotNet_10.Tests;

// [Feature05] 수식어 붙은 람다 매개변수 — out 람다가 실제로 파싱하는지 확인
public class LambdaParameterModifiersTests
{
    [Fact]
    public void 파싱_성공시_true와_결과값을_준다()
    {
        var parser = LambdaParameterModifiersDemo.IntParser;

        var ok = parser("123", out var result);

        ok.Should().BeTrue();
        result.Should().Be(123);
    }

    [Fact]
    public void 파싱_실패시_false와_기본값을_준다()
    {
        var parser = LambdaParameterModifiersDemo.IntParser;

        var ok = parser("not-a-number", out var result);

        ok.Should().BeFalse();
        result.Should().Be(0);
    }
}
