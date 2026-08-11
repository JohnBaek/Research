using AwesomeAssertions;
using DotNet_10.Features;

namespace DotNet_10.Tests;

// [Feature01] field keyword - check that the validation inside the accessor works
public class FieldKeywordTests
{
    [Fact]
    public void Field_Keyword_Setter_Null()
    {
        var t = new Temperature { Celsius = 36.5 };
        t.Celsius.Should().Be(36.5);
    }

    [Fact]
    public void Celsius_stores_a_valid_value_as_is()
    {
        var t = new Temperature { Celsius = 36.5 };

        t.Celsius.Should().Be(36.5);
    }

    [Fact]
    public void Celsius_below_absolute_zero_throws()
    {
        // On assignment, the set accessor's validation (using 'field') runs.
        var act = () => new Temperature { Celsius = -300 };

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Unit_default_is_the_celsius_symbol()
    {
        new Temperature().Unit.Should().Be("℃");
    }

    [Theory]
    [InlineData("   ")]
    [InlineData("")]
    public void Unit_falls_back_to_default_when_blank(string blank)
    {
        var t = new Temperature { Unit = blank };

        t.Unit.Should().Be("℃");
    }
}
