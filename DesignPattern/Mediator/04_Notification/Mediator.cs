// A notification: a message with NO response. Many handlers may react to it.
public interface INotification;

// Handles a notification type. There can be many handlers per type.
public interface INotificationHandler<TNotification>
    where TNotification : INotification
{
    void Handle(TNotification notification);
}

// The mediator: publish a notification to every subscribed handler.
public interface IMediator
{
    void Publish<TNotification>(TNotification notification)
        where TNotification : INotification;
}

// Minimal hand-rolled mediator for one-to-many delivery.
public sealed class Mediator : IMediator
{
    // One notification type -> MANY handlers.
    private readonly Dictionary<Type, List<object>> _handlers = [];

    public void Subscribe<TNotification>(INotificationHandler<TNotification> handler)
        where TNotification : INotification
    {
        if (!_handlers.TryGetValue(typeof(TNotification), out var list))
            _handlers[typeof(TNotification)] = list = [];

        list.Add(handler);
    }

    public void Publish<TNotification>(TNotification notification)
        where TNotification : INotification
    {
        if (!_handlers.TryGetValue(typeof(TNotification), out var list))
            return; // no subscribers, nothing to do

        // Fan out: call every handler registered for this type.
        foreach (var handler in list)
            ((INotificationHandler<TNotification>)handler).Handle(notification);
    }
}
