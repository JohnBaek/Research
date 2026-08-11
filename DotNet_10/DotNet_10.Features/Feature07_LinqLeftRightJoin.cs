namespace DotNet_10.Features;

/// <summary>사용자. 데모용 모델.</summary>
public record User(int Id, string Name);

/// <summary>구매 내역. UserId로 User와 연결된다.</summary>
public record Purchase(int UserId, string Product);

/// <summary>
/// [.NET 10 BCL] LINQ LeftJoin / RightJoin
///
/// 기존 Enumerable.Join은 "내부 조인(inner join)"이라 양쪽에 매칭되는
/// 항목만 결과에 남았다. 매칭 안 되는 쪽을 살리려면 GroupJoin +
/// SelectMany + DefaultIfEmpty 를 조합해야 해서 번거로웠다.
///
/// .NET 10부터 LeftJoin / RightJoin 이 표준 LINQ 메서드로 추가됐다.
///   - LeftJoin : 왼쪽(첫 번째) 시퀀스는 전부 유지. 매칭 없으면 오른쪽은 default(null).
///   - RightJoin: 오른쪽(두 번째) 시퀀스는 전부 유지. 매칭 없으면 왼쪽은 default(null).
/// </summary>
public static class LinqJoinDemo
{
    /// <summary>
    /// 모든 사용자를 유지하면서 구매 상품을 붙인다.
    /// 구매가 없는 사용자는 Product 가 null 로 남는다.
    /// </summary>
    public static IEnumerable<(string User, string? Product)> UsersWithPurchases(
        IEnumerable<User> users, IEnumerable<Purchase> purchases) =>
        users.LeftJoin(
            purchases,
            user => user.Id,        // 왼쪽 키
            buy => buy.UserId,      // 오른쪽 키
            (user, buy) => (user.Name, buy?.Product)); // buy는 매칭 없으면 null

    /// <summary>
    /// 모든 구매를 유지하면서 사용자 이름을 붙인다.
    /// 사용자를 못 찾은 구매는 User 가 null 로 남는다("고아" 구매 탐지에 유용).
    /// </summary>
    public static IEnumerable<(string? User, string Product)> PurchasesWithUsers(
        IEnumerable<User> users, IEnumerable<Purchase> purchases) =>
        users.RightJoin(
            purchases,
            user => user.Id,
            buy => buy.UserId,
            (user, buy) => (user?.Name, buy.Product)); // user는 매칭 없으면 null
}
