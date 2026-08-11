using AwesomeAssertions;
using DotNet_10.Features;

namespace DotNet_10.Tests;

// [Feature01] field 키워드 — 접근자 안 검증 로직이 실제로 동작하는지 확인
public class FieldKeywordTests
{
    [Fact]
    public void Field_Keyword_Setter_Null()
    {
        var t = new Temperature { Celsius = 36.5 };
        t.Celsius.Should().Be(36.5);
    }
    
    [Fact]
    public void Celsius_정상값은_그대로_저장된다()
    {
        var t = new Temperature { Celsius = 36.5 };

        t.Celsius.Should().Be(36.5);
    }

    [Fact]
    public void Celsius_절대영도_미만이면_예외를_던진다()
    {
        // 대입 시점에 set 접근자의 검증(field 사용)이 동작한다.
        var act = () => new Temperature { Celsius = -300 };

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Unit_초기값은_섭씨기호다()
    {
        new Temperature().Unit.Should().Be("℃");
    }

    [Theory]
    [InlineData("   ")]
    [InlineData("")]
    public void Unit_공백이면_기본단위로_되돌린다(string blank)
    {
        var t = new Temperature { Unit = blank };

        t.Unit.Should().Be("℃");
    }
}
