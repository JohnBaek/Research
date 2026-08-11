using AwesomeAssertions;
using DotNet_10.Features;

namespace DotNet_10.Tests;

// [Feature08] Shuffle — 원소 보존 + 원본 불변 확인 (순서 랜덤이라 순서 자체는 단정 안 함)
public class LinqShuffleTests
{
    [Fact]
    public void Shuffle_는_같은_원소집합을_보존한다()
    {
        var source = Enumerable.Range(1, 100).ToArray();

        var shuffled = LinqShuffleDemo.Shuffled(source);

        // 순서와 무관하게 같은 원소들이 그대로 있어야 한다.
        shuffled.Should().BeEquivalentTo(source);
    }

    [Fact]
    public void Shuffle_는_원본을_변경하지_않는다()
    {
        var source = new[] { 1, 2, 3, 4, 5 };
        var snapshot = source.ToArray();

        _ = LinqShuffleDemo.Shuffled(source);

        source.Should().Equal(snapshot); // 원본 순서 그대로
    }
}
