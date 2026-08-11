// Mediator pattern - Step 2: Classic GoF Mediator
//
// Now users (Colleagues) do NOT know each other. Each one knows only a
// single Mediator (the ChatRoom). To broadcast, a user hands the message
// to the mediator, and the mediator decides who receives it.
//
// Roles (GoF):
//   - Mediator  (IChatMediator): how colleagues communicate.
//   - Colleague (User): knows only its mediator, never another colleague.
//
// Result: adding/removing a user touches only the mediator, not every user.

var room = new ChatRoom();

var alice = new User("Alice");
var bob = new User("Bob");
var carol = new User("Carol");

// Register once with the mediator - no user-to-user wiring needed.
room.Register(alice);
room.Register(bob);
room.Register(carol);

alice.Send("Hi everyone!");
bob.Send("Hello Alice!");
