using AwesomeAssertions;
using DotNet_10.Features;

namespace DotNet_10.Tests;

// [Feature06] User-defined compound assignment - check += / -= mutate in place
public class UserDefinedCompoundAssignmentTests
{
    [Fact]
    public void PlusEquals_accumulates_in_place()
    {
        var acc = new Accumulator(10);

        acc += 5;

        acc.Total.Should().Be(15);
    }

    [Fact]
    public void MinusEquals_decreases_in_place()
    {
        var acc = new Accumulator(10);

        acc -= 3;

        acc.Total.Should().Be(7);
    }
}
