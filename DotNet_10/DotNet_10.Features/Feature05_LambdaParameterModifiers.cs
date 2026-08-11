namespace DotNet_10.Features;

/// <summary>Delegate that tries to parse text into T (with an out parameter).</summary>
public delegate bool TryParse<T>(string text, out T result);

/// <summary>
/// [C# 14 / .NET 10] Simple lambda parameters with modifiers.
///
/// Before, to put a modifier (ref/out/in/scoped) on a lambda parameter,
/// you also had to write the parameter "type":
///   (string text, out int result) => ...
///
/// From C# 14 you can drop the type and keep just the modifier:
///   (text, out result) => ...
///
/// The types are inferred from the target delegate signature.
/// </summary>
public static class LambdaParameterModifiersDemo
{
    /// <summary>
    /// Keeps the 'out' modifier but omits the types (string, int).
    /// TryParse&lt;int&gt; infers text=string and result=int.
    /// </summary>
    public static TryParse<int> IntParser =>
        (text, out result) => int.TryParse(text, out result);
}
