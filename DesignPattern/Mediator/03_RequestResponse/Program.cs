// Mediator pattern - Step 3: Request/Response mediator (application style)
//
// This is the style used by libraries like MediatR. Instead of objects
// talking to each other, the caller builds a "request" object and hands it
// to a mediator. The mediator finds the ONE matching handler and returns
// its result:
//
//   caller -> IMediator.Send(request) -> (mediator picks the handler) -> response
//
// Benefit: the caller is fully decoupled from the handler class.

var mediator = new Mediator();

// Wire each request type to its handler (once, at startup).
mediator.Register(new GreetHandler());
mediator.Register(new AddHandler());

// The caller only builds a request and sends it - it never touches a handler.
string greeting = mediator.Send(new Greet("Alice"));
int sum = mediator.Send(new Add(3, 4));

Console.WriteLine(greeting);      // Hello, Alice!
Console.WriteLine($"3 + 4 = {sum}"); // 3 + 4 = 7
