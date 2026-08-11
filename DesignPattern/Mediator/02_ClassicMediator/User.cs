// Colleague role: a user knows ONLY its mediator, not other users.
// Compare with Step 1, where a user held a list of every other user.
public sealed class User(string name)
{
    private IChatMediator? _mediator;

    public string Name { get; } = name;

    public void SetMediator(IChatMediator mediator) => _mediator = mediator;

    public void Send(string message)
    {
        Console.WriteLine($"[{Name} sends] {message}");

        // Just hand the message to the mediator. The user does not know
        // who the receivers are or how many there are.
        _mediator!.Broadcast(Name, message);
    }

    public void Receive(string from, string message) =>
        Console.WriteLine($"    {Name} got from {from}: {message}");
}
