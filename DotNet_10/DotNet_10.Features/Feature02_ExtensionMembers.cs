namespace DotNet_10.Features;

/// <summary>
/// [C# 14 / .NET 10] 확장 멤버 (Extension Members)
///
/// 기존 확장 메서드(static method + this 매개변수)를 넘어서,
/// 확장 "프로퍼티"와 확장 "정적 멤버"까지 선언할 수 있게 됐다.
///
/// 핵심 문법:
///   static class 안에  extension(수신자타입 이름) { ... }  블록을 두고,
///   그 블록 안에 프로퍼티/메서드를 일반 멤버처럼 선언한다.
///   블록의 "수신자(receiver)"가 곧 this 대상이 된다.
///
/// 장점:
///   - 확장 프로퍼티가 가능해져서, 계산된 값을 메서드가 아니라
///     프로퍼티처럼 자연스럽게 노출할 수 있다 (str.IsBlank).
/// </summary>
public static class StringExtensions
{
    // 수신자: string source  → 아래 멤버들은 모두 string에 대한 확장이 된다.
    extension(string source)
    {
        /// <summary>확장 프로퍼티 — C# 14에서 새로 가능해진 형태.</summary>
        public bool IsBlank => string.IsNullOrWhiteSpace(source);

        /// <summary>공백 기준 단어 개수를 세는 확장 프로퍼티.</summary>
        public int WordCount =>
            source.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;

        /// <summary>확장 메서드도 동일한 블록 안에 자연스럽게 함께 둔다.</summary>
        public string Repeat(int count) =>
            count <= 0 ? string.Empty : string.Concat(Enumerable.Repeat(source, count));
    }
}
