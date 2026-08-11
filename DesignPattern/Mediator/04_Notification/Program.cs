// Mediator pattern - Step 4: Notification (publish/subscribe, one-to-many)
//
// Sometimes one event should reach MANY handlers. A notification is a
// message with no response; the mediator delivers it to every handler
// subscribed to that type. This is the pub/sub side of MediatR.
//
//   publisher -> IMediator.Publish(event) -> handler A, handler B, handler C ...
//
// The publisher does not know how many handlers exist or what they do.

var mediator = new Mediator();

// Three independent handlers subscribe to the same event.
mediator.Subscribe(new SendEmailHandler());
mediator.Subscribe(new UpdateInventoryHandler());
mediator.Subscribe(new AuditLogHandler());

Console.WriteLine("Publishing OrderPlaced...");

// One publish call -> all three handlers react.
mediator.Publish(new OrderPlaced("Keyboard", 2));
