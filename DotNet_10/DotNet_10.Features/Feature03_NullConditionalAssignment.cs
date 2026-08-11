namespace DotNet_10.Features;

/// <summary>주문. 데모용 단순 모델.</summary>
public class Order
{
    public int Quantity { get; set; }
}

/// <summary>
/// [C# 14 / .NET 10] 널 조건부 대입 (Null-conditional assignment)
///
/// 그동안 ?. 연산자는 "읽기"에서만 쓸 수 있었다.
/// C# 14부터는 대입(=)과 복합 대입(+= 등)의 "왼쪽"에도 쓸 수 있다.
///
///   target?.Member = value;
///
/// 의미: target 이 null이 아니면 대입을 수행하고,
///       null이면 대입 자체를 건너뛴다(그리고 오른쪽 식도 평가되지 않는다).
///
/// 이전엔 이렇게 써야 했다:
///   if (target is not null) target.Member = value;
/// </summary>
public static class NullConditionalAssignmentDemo
{
    /// <summary>
    /// order 가 null이면 아무 일도 일어나지 않는다 (NullReferenceException 없음).
    /// order 가 null이 아니면 Quantity를 갱신한다.
    /// </summary>
    public static void UpdateQuantity(Order? order, int quantity)
    {
        order?.Quantity = quantity;
    }

    /// <summary>
    /// 오른쪽 식이 "평가되지 않는" 점을 보여주는 예.
    /// order 가 null이면 sideEffect() 는 호출조차 되지 않는다.
    /// </summary>
    public static void UpdateQuantity(Order? order, Func<int> sideEffect)
    {
        order?.Quantity = sideEffect();
    }
}
