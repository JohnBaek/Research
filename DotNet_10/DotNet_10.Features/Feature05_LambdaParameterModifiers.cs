namespace DotNet_10.Features;

/// <summary>문자열을 T로 파싱 시도하는 델리게이트 (out 매개변수 포함).</summary>
public delegate bool TryParse<T>(string text, out T result);

/// <summary>
/// [C# 14 / .NET 10] 수식어가 붙은 단순 람다 매개변수
/// (Simple lambda parameters with modifiers)
///
/// 이전엔 람다 매개변수에 ref/out/in/scoped 같은 수식어를 붙이려면,
/// 반드시 매개변수의 "타입"까지 함께 적어야 했다:
///   (string text, out int result) => ...
///
/// C# 14부터는 타입을 생략하고 수식어만 붙일 수 있다:
///   (text, out result) => ...
///
/// 타입은 대상 델리게이트 시그니처에서 추론된다.
/// </summary>
public static class LambdaParameterModifiersDemo
{
    /// <summary>
    /// out 수식어를 유지하면서 매개변수 타입(string, int)은 생략했다.
    /// 델리게이트 TryParse&lt;int&gt; 로부터 text=string, result=int 가 추론된다.
    /// </summary>
    public static TryParse<int> IntParser =>
        (text, out result) => int.TryParse(text, out result);
}
