using AwesomeAssertions;
using DotNet_10.Features;

namespace DotNet_10.Tests;

// [Feature07] LeftJoin / RightJoin - check the unmatched side is kept as null
public class LinqLeftRightJoinTests
{
    private static readonly User[] Users =
    [
        new(1, "kim"),
        new(2, "lee"),
        new(3, "park"), // no purchase -> Product is null in LeftJoin
    ];

    private static readonly Purchase[] Purchases =
    [
        new(1, "keyboard"),
        new(2, "mouse"),
        new(99, "ghost"), // no user -> User is null in RightJoin
    ];

    [Fact]
    public void LeftJoin_keeps_all_users_and_null_when_no_purchase()
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
    public void RightJoin_keeps_all_purchases_and_null_when_no_user()
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
