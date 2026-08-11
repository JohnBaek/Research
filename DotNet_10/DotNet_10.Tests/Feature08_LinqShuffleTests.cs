using AwesomeAssertions;
using DotNet_10.Features;

namespace DotNet_10.Tests;

// [Feature08] Shuffle - check elements are preserved and source is unchanged (order is random, so not asserted)
public class LinqShuffleTests
{
    [Fact]
    public void Shuffle_preserves_the_same_set_of_elements()
    {
        var source = Enumerable.Range(1, 100).ToArray();

        var shuffled = LinqShuffleDemo.Shuffled(source);

        // Same elements must be present, regardless of order.
        shuffled.Should().BeEquivalentTo(source);
    }

    [Fact]
    public void Shuffle_does_not_change_the_source()
    {
        var source = new[] { 1, 2, 3, 4, 5 };
        var snapshot = source.ToArray();

        _ = LinqShuffleDemo.Shuffled(source);

        source.Should().Equal(snapshot); // source order stays the same
    }
}
