namespace DotNet_10.Features;

/// <summary>An order. A simple demo model.</summary>
public class Order
{
    public int Quantity { get; set; }
}

/// <summary>
/// [C# 14 / .NET 10] Null-conditional assignment.
///
/// Until now the ?. operator worked only for "reading".
/// From C# 14 you can also use it on the "left" side of an assignment (=)
/// and compound assignment (+= etc.):
///
///   target?.Member = value;
///
/// Meaning: if target is not null, do the assignment; if it is null,
///          skip the assignment (and the right-hand side is not evaluated).
///
/// Before you had to write:
///   if (target is not null) target.Member = value;
/// </summary>
public static class NullConditionalAssignmentDemo
{
    /// <summary>
    /// If order is null, nothing happens (no NullReferenceException).
    /// If order is not null, Quantity is updated.
    /// </summary>
    public static void UpdateQuantity(Order? order, int quantity)
    {
        order?.Quantity = quantity;
    }

    /// <summary>
    /// Shows that the right-hand side is "not evaluated".
    /// If order is null, sideEffect() is never even called.
    /// </summary>
    public static void UpdateQuantity(Order? order, Func<int> sideEffect)
    {
        order?.Quantity = sideEffect();
    }
}
