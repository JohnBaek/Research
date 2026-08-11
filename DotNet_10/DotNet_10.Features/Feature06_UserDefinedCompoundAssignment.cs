namespace DotNet_10.Features;

/// <summary>
/// [C# 14 / .NET 10] 사용자 정의 복합 대입 연산자
/// (User-defined compound assignment operators)
///
/// 이전엔 x += y 를 컴파일러가 항상 x = x + y 로 바꿔 처리했다.
/// 즉 새 인스턴스를 만들어 다시 대입하는 식이라, 큰 값 타입에선 비효율적일 수 있었다.
///
/// C# 14부터는 += 같은 복합 대입 연산자를 "인스턴스 연산자"로 직접 정의해서,
/// 새 인스턴스를 만들지 않고 "제자리(in place)"에서 상태를 바꾸도록 할 수 있다.
///   public void operator +=(피연산자) { ... }   // 반환형 void, 인스턴스 자신을 수정
/// </summary>
public struct Accumulator(int initial)
{
    /// <summary>누적 합. 데모를 위해 필드로 공개.</summary>
    public int Total = initial;

    /// <summary>
    /// += 를 제자리 수정으로 정의. (this 를 복제하지 않고 Total 만 증가)
    /// </summary>
    public void operator +=(int amount) => Total += amount;

    /// <summary>-= 도 대칭적으로 정의.</summary>
    public void operator -=(int amount) => Total -= amount;
}
