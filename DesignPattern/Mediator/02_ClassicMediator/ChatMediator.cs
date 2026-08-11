// Mediator role: defines how colleagues communicate, so colleagues
// never reference each other directly.
public interface IChatMediator
{
    void Register(User user);
    void Broadcast(string from, string message);
}

// Concrete mediator: it owns the list of users and routes messages.
// All the "who talks to whom" logic lives here, in one place.
public sealed class ChatRoom : IChatMediator
{
    private readonly List<User> _users = [];

    public void Register(User user)
    {
        _users.Add(user);
        user.SetMediator(this); // the user only ever knows this mediator
    }

    public void Broadcast(string from, string message)
    {
        // Deliver to everyone except the sender.
        foreach (var user in _users)
            if (user.Name != from)
                user.Receive(from, message);
    }
}
