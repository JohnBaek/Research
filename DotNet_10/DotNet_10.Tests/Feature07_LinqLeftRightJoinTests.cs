using AwesomeAssertions;
using DotNet_10.Features;

namespace DotNet_10.Tests;

// [Feature07] LeftJoin / RightJoin — 매칭 없는 쪽이 null로 보존되는지 확인
public class LinqLeftRightJoinTests
{
    private static readonly User[] Users =
    [
        new(1, "kim"),
        new(2, "lee"),
        new(3, "park"), // 구매 없음 → LeftJoin에서 Product null
    ];

    private static readonly Purchase[] Purchases =
    [
        new(1, "keyboard"),
        new(2, "mouse"),
        new(99, "ghost"), // 사용자 없음 → RightJoin에서 User null
    ];

    [Fact]
    public void LeftJoin_모든_사용자를_유지하고_구매없으면_null()
    {
        var result = LinqJoinDemo.UsersWithPurchases(Users, Purchases).ToArray();

        result.Should().BeEquivalentTo(new (string, string?)[]
        {
            ("kim", "keyboard"),
            ("lee", "mouse"),
            ("park", null),
        });
    }

    [Fact]
    public void RightJoin_모든_구매를_유지하고_사용자없으면_null()
    {
        var result = LinqJoinDemo.PurchasesWithUsers(Users, Purchases).ToArray();

        result.Should().BeEquivalentTo(new (string?, string)[]
        {
            ("kim", "keyboard"),
            ("lee", "mouse"),
            (null, "ghost"),
        });
    }
}
