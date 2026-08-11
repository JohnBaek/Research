namespace DotNet_10.Features;

/// <summary>
/// [C# 14 / .NET 10] Extension Members.
///
/// Beyond the classic extension method (a static method with a 'this'
/// parameter), you can now declare extension "properties" and static members.
///
/// Syntax:
///   Inside a static class, write an  extension(ReceiverType name) { ... }  block,
///   then declare properties/methods like normal members. The block's
///   "receiver" becomes the 'this' target.
///
/// Benefit:
///   - Extension properties let you expose a computed value like a property
///     (str.IsBlank) instead of a method.
/// </summary>
public static class StringExtensions
{
    // Receiver: string source -> every member below extends string.
    extension(string source)
    {
        /// <summary>Extension property - newly possible in C# 14.</summary>
        public bool IsBlank => string.IsNullOrWhiteSpace(source);

        /// <summary>Extension property that counts words split by whitespace.</summary>
        public int WordCount =>
            source.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;

        /// <summary>An extension method lives naturally in the same block.</summary>
        public string Repeat(int count) =>
            count <= 0 ? string.Empty : string.Concat(Enumerable.Repeat(source, count));
    }
}
