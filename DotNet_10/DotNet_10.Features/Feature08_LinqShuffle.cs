namespace DotNet_10.Features;

/// <summary>
/// [.NET 10 BCL] LINQ Shuffle.
///
/// Returns a "new sequence" with the elements in random order.
/// Before, you used tricks like OrderBy(_ => Guid.NewGuid()) or wrote
/// Fisher-Yates by hand. .NET 10 provides Enumerable.Shuffle as standard.
///
/// Notes:
///   - It does not modify the source (a new, lazily-evaluated sequence).
///   - The set of elements is kept; only the order becomes random.
/// </summary>
public static class LinqShuffleDemo
{
    /// <summary>Return a shuffled new list from source. (source stays unchanged)</summary>
    public static IReadOnlyList<T> Shuffled<T>(IEnumerable<T> source) =>
        source.Shuffle().ToArray();
}
