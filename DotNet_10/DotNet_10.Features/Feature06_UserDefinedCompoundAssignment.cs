namespace DotNet_10.Features;

/// <summary>
/// [C# 14 / .NET 10] User-defined compound assignment operators.
///
/// Before, the compiler always rewrote x += y as x = x + y.
/// That builds a new instance and assigns it back, which can be wasteful
/// for large value types.
///
/// From C# 14 you can define compound operators like += as "instance
/// operators", so they change state "in place" without a new instance:
///   public void operator +=(operand) { ... }   // returns void, mutates 'this'
/// </summary>
public struct Accumulator(int initial)
{
    /// <summary>Running total. Public field for the demo.</summary>
    public int Total = initial;

    /// <summary>Defines += as an in-place update (no copy of 'this', just Total).</summary>
    public void operator +=(int amount) => Total += amount;

    /// <summary>Defines -= symmetrically.</summary>
    public void operator -=(int amount) => Total -= amount;
}
