using AwesomeAssertions;
using DotNet_10.Features;

namespace DotNet_10.Tests;

// [Feature02] 확장 멤버 — 확장 프로퍼티/메서드가 string에 그대로 붙는지 확인
public class ExtensionMembersTests
{
    [Theory]
    [InlineData("", true)]
    [InlineData("   ", true)]
    [InlineData("hello", false)]
    public void IsBlank_확장프로퍼티가_동작한다(string input, bool expected)
    {
        input.IsBlank.Should().Be(expected);
    }

    [Fact]
    public void WordCount_단어개수를_센다()
    {
        "the quick brown fox".WordCount.Should().Be(4);
    }

    [Fact]
    public void Repeat_문자열을_n번_반복한다()
    {
        "ab".Repeat(3).Should().Be("ababab");
    }

    [Fact]
    public void Repeat_0이하면_빈문자열이다()
    {
        "ab".Repeat(0).Should().BeEmpty();
    }
}
