using AwesomeAssertions;
using DotNet_10.Features;

namespace DotNet_10.Tests;

// [Feature02] Extension members - check extension property/method attach to string
public class ExtensionMembersTests
{
    [Theory]
    [InlineData("", true)]
    [InlineData("   ", true)]
    [InlineData("hello", false)]
    public void IsBlank_extension_property_works(string input, bool expected)
    {
        input.IsBlank.Should().Be(expected);
    }

    [Fact]
    public void WordCount_counts_the_words()
    {
        "the quick brown fox".WordCount.Should().Be(4);
    }

    [Fact]
    public void Repeat_repeats_the_string_n_times()
    {
        "ab".Repeat(3).Should().Be("ababab");
    }

    [Fact]
    public void Repeat_returns_empty_when_count_is_zero_or_less()
    {
        "ab".Repeat(0).Should().BeEmpty();
    }
}
