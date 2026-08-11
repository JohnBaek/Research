namespace DotNet_10.Features;

/// <summary>
/// [C# 14 / .NET 10] The 'field' keyword (a contextual keyword).
///
/// Inside a property accessor (get/set), you can now access the
/// compiler-generated "backing field" directly by the name <c>field</c>.
///
/// Before (C# 13 and earlier):
///   - To add logic like validation, you had to declare a private field
///     by hand and wire the property to read/write it.
/// Now (C# 14):
///   - Use 'field' in the accessor with no field declaration -> less boilerplate.
///
/// Note: 'field' is a contextual keyword. If a variable named 'field' is in
///       scope, that variable wins (for compatibility). So avoid naming things 'field'.
/// </summary>
public class Temperature
{
    /// <summary>
    /// Celsius temperature that rejects values below absolute zero (-273.15).
    /// No backing field is declared; 'field' adds validation directly.
    /// </summary>
    public double Celsius
    {
        get => field;
        set
        {
            if (value < -273.15)
                throw new ArgumentOutOfRangeException(
                    nameof(value), value, "Cannot be below absolute zero (-273.15 C).");

            field = value;
        }
    }

    /// <summary>
    /// A property initializer (= "℃") also works on a field-backed property.
    /// Blank/whitespace input falls back to the default unit.
    /// </summary>
    public string Unit
    {
        get => field;
        set => field = string.IsNullOrWhiteSpace(value) ? "℃" : value.Trim();
    } = "℃";
}
