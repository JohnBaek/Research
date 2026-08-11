// Mediator pattern - Step 1: The PROBLEM (no mediator)
//
// A chat room where each User holds direct references to every other User.
// To send a message, a user loops over its own peer list and calls each peer.
//
// Why this hurts:
//   - Every user must know every other user (N-to-N references).
//   - Adding or removing a user means editing many peer lists.
//   - The "who talks to whom" logic is scattered across all users.
//
// The Mediator pattern (next steps) fixes this by moving that logic
// into one central place.

var alice = new User("Alice");
var bob = new User("Bob");
var carol = new User("Carol");

// Everyone must be wired to everyone else by hand. This is the coupling problem.
alice.Connect(bob);
alice.Connect(carol);
bob.Connect(alice);
bob.Connect(carol);
carol.Connect(alice);
carol.Connect(bob);

alice.Send("Hi everyone!");
bob.Send("Hello Alice!");

// A user that talks to other users directly.
sealed class User(string name)
{
    // Direct references to other users -> tight coupling.
    private readonly List<User> _peers = [];

    public string Name { get; } = name;

    // Each user keeps its own peer list.
    public void Connect(User peer) => _peers.Add(peer);

    public void Send(string message)
    {
        Console.WriteLine($"[{Name} sends] {message}");

        // The sender itself must loop over every peer and deliver.
        foreach (var peer in _peers)
            peer.Receive(Name, message);
    }

    public void Receive(string from, string message) =>
        Console.WriteLine($"    {Name} got from {from}: {message}");
}
