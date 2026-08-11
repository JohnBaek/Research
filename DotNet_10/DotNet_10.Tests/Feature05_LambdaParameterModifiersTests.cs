using AwesomeAssertions;
using DotNet_10.Features;

namespace DotNet_10.Tests;

// [Feature05] Lambda parameters with modifiers - check the 'out' lambda actually parses
public class LambdaParameterModifiersTests
{
    [Fact]
    public void On_success_it_returns_true_and_the_parsed_value()
    {
        var parser = LambdaParameterModifiersDemo.IntParser;

        var ok = parser("123", out var result);

        ok.Should().BeTrue();
        result.Should().Be(123);
    }

    [Fact]
    public void On_failure_it_returns_false_and_the_default_value()
    {
        var parser = LambdaParameterModifiersDemo.IntParser;

        var ok = parser("not-a-number", out var result);

        ok.Should().BeFalse();
        result.Should().Be(0);
    }
}
