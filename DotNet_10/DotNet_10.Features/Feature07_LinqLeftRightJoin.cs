namespace DotNet_10.Features;

/// <summary>A user. Demo model.</summary>
public record User(int Id, string Name);

/// <summary>A purchase, linked to a User by UserId.</summary>
public record Purchase(int UserId, string Product);

/// <summary>
/// [.NET 10 BCL] LINQ LeftJoin / RightJoin.
///
/// The existing Enumerable.Join is an "inner join": only items matched on
/// both sides survive. To keep unmatched items you had to combine
/// GroupJoin + SelectMany + DefaultIfEmpty, which was awkward.
///
/// .NET 10 adds LeftJoin / RightJoin as standard LINQ methods:
///   - LeftJoin : keep every left (first) item. No match -> right is default (null).
///   - RightJoin: keep every right (second) item. No match -> left is default (null).
/// </summary>
public static class LinqJoinDemo
{
    /// <summary>
    /// Keep all users and attach their purchased product.
    /// A user with no purchase gets a null Product.
    /// </summary>
    public static IEnumerable<(string User, string? Product)> UsersWithPurchases(
        IEnumerable<User> users, IEnumerable<Purchase> purchases) =>
        users.LeftJoin(
            purchases,
            user => user.Id,        // left key
            buy => buy.UserId,      // right key
            (user, buy) => (user.Name, buy?.Product)); // buy is null when unmatched

    /// <summary>
    /// Keep all purchases and attach the user name.
    /// A purchase with no matching user gets a null User (useful to find "orphans").
    /// </summary>
    public static IEnumerable<(string? User, string Product)> PurchasesWithUsers(
        IEnumerable<User> users, IEnumerable<Purchase> purchases) =>
        users.RightJoin(
            purchases,
            user => user.Id,
            buy => buy.UserId,
            (user, buy) => (user?.Name, buy.Product)); // user is null when unmatched
}
