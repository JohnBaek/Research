namespace DotNet_10.Features;

/// <summary>
/// [.NET 10 BCL] LINQ Shuffle
///
/// 시퀀스의 원소를 무작위 순서로 섞은 "새 시퀀스"를 돌려준다.
/// 이전엔 OrderBy(_ => Guid.NewGuid()) 같은 편법이나 직접 Fisher-Yates를
/// 구현해야 했는데, .NET 10부터 Enumerable.Shuffle 로 표준 제공된다.
///
/// 특징:
///   - 원본 컬렉션을 변경하지 않는다(지연 실행되는 새 시퀀스).
///   - 원소 집합은 그대로 보존되고, 순서만 랜덤하게 바뀐다.
/// </summary>
public static class LinqShuffleDemo
{
    /// <summary>source를 섞은 새 리스트를 만든다. (원본 불변)</summary>
    public static IReadOnlyList<T> Shuffled<T>(IEnumerable<T> source) =>
        source.Shuffle().ToArray();
}
