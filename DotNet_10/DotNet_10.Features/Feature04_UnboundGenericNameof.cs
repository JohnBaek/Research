namespace DotNet_10.Features;

/// <summary>
/// [C# 14 / .NET 10] nameof 에서 언바운드(제네릭) 타입 사용
///
/// 이전엔 nameof 안에서 제네릭 타입의 타입 인자를 반드시 채워야 했다:
///   nameof(List&lt;int&gt;)   // OK
///   nameof(List&lt;&gt;)      // 컴파일 에러였음
///
/// C# 14부터는 "언바운드 제네릭"(타입 인자를 비운 형태)을 허용한다.
/// 반환되는 이름에는 타입 인자가 포함되지 않는다 → "List", "Dictionary".
///
/// 활용: 특정 타입 인자와 무관하게 "제네릭 타입 자체의 이름"이 필요할 때
///       (로깅, 진단 메시지, 리플렉션 보조 등).
/// </summary>
public static class UnboundGenericNameofDemo
{
    /// <summary>nameof(List&lt;&gt;) → "List"</summary>
    public static string ListName => nameof(List<>);

    /// <summary>nameof(Dictionary&lt;,&gt;) → "Dictionary" (콤마로 타입 인자 개수 표현)</summary>
    public static string DictionaryName => nameof(Dictionary<,>);
}
