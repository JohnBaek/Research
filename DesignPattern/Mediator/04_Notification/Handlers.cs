// One event, several reactions. Each handler does its own job and knows
// nothing about the others. This is the pub/sub side of the mediator.

// The event: an order was placed.
public sealed record OrderPlaced(string Product, int Quantity) : INotification;

// Reaction 1: send a confirmation email.
public sealed class SendEmailHandler : INotificationHandler<OrderPlaced>
{
    public void Handle(OrderPlaced n) =>
        Console.WriteLine($"  [Email]     Confirmation sent: {n.Quantity} x {n.Product}");
}

// Reaction 2: reduce stock.
public sealed class UpdateInventoryHandler : INotificationHandler<OrderPlaced>
{
    public void Handle(OrderPlaced n) =>
        Console.WriteLine($"  [Inventory] Stock of {n.Product} reduced by {n.Quantity}");
}

// Reaction 3: write an audit log.
public sealed class AuditLogHandler : INotificationHandler<OrderPlaced>
{
    public void Handle(OrderPlaced n) =>
        Console.WriteLine($"  [Audit]     Order logged: {n.Product}");
}
