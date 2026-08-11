namespace DotNet_10.Features;

/// <summary>
/// [C# 14 / .NET 10] Unbound generic types in nameof.
///
/// Before, nameof required you to fill in the type arguments of a generic type:
///   nameof(List&lt;int&gt;)   // OK
///   nameof(List&lt;&gt;)      // was a compile error
///
/// From C# 14 an "unbound generic" (empty type arguments) is allowed.
/// The returned name has no type arguments -> "List", "Dictionary".
///
/// Use case: when you need the "name of the generic type itself", regardless
///           of type arguments (logging, diagnostics, reflection helpers).
/// </summary>
public static class UnboundGenericNameofDemo
{
    /// <summary>nameof(List&lt;&gt;) -> "List"</summary>
    public static string ListName => nameof(List<>);

    /// <summary>nameof(Dictionary&lt;,&gt;) -> "Dictionary" (comma marks the arity)</summary>
    public static string DictionaryName => nameof(Dictionary<,>);
}
