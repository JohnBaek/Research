namespace DotNet_10.Features;

/// <summary>
/// [C# 14 / .NET 10] field 키워드 (contextual keyword)
///
/// 프로퍼티 접근자(get/set) 안에서, 컴파일러가 자동 생성한
/// "backing field"를 <c>field</c> 라는 이름으로 직접 참조할 수 있다.
///
/// 이전(C# 13 이하):
///   - 검증 로직 등이 필요하면 private 필드를 손으로 선언하고
///     프로퍼티가 그 필드를 읽고 쓰도록 직접 연결해야 했다.
/// 지금(C# 14):
///   - 필드 선언 없이 접근자 안에서 field 로 바로 접근 → 보일러플레이트 감소.
///
/// 주의: field 는 "컨텍스트 키워드"라서, field 라는 이름의 변수가
///       스코프에 있으면 그 변수가 우선한다(호환성). 그래서 필드명을 field로
///       짓는 건 피하는 게 좋다.
/// </summary>
public class Temperature
{
    /// <summary>
    /// 절대영도(-273.15℃) 미만은 허용하지 않는 섭씨 온도.
    /// backing field를 직접 선언하지 않고 field 키워드로 검증만 추가했다.
    /// </summary>
    public double Celsius
    {
        get => field;
        set
        {
            if (value < -273.15)
                throw new ArgumentOutOfRangeException(
                    nameof(value), value, "절대영도(-273.15℃)보다 낮을 수 없습니다.");

            field = value;
        }
    }

    /// <summary>
    /// 프로퍼티 초기화 구문(= "℃")도 field 기반 프로퍼티에서 그대로 동작한다.
    /// 공백/빈 문자열이 들어오면 기본 단위로 되돌린다.
    /// </summary>
    public string Unit
    {
        get => field;
        set => field = string.IsNullOrWhiteSpace(value) ? "℃" : value.Trim();
    } = "℃";
}
