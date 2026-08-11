# Mediator Pattern

Four small console projects that build up the Mediator pattern step by step.
Each one runs on its own and prints its message flow.

## The idea

The Mediator pattern removes direct references between objects. Instead of
objects talking to each other (N-to-N coupling), they talk through one
central mediator. This keeps the "who talks to whom" logic in a single place.

## Projects

| # | Project | What it teaches |
|---|---------|-----------------|
| 01 | `WithoutMediator` | The **problem**: each user references every other user directly |
| 02 | `ClassicMediator` | **GoF Mediator**: users know only the mediator, not each other |
| 03 | `RequestResponse` | **Send a request, get a response** (the MediatR style) |
| 04 | `Notification` | **Publish to many handlers**, one-to-many (pub/sub) |

## Run

```bash
dotnet run --project 01_WithoutMediator/WithoutMediator.csproj
dotnet run --project 02_ClassicMediator/ClassicMediator.csproj
dotnet run --project 03_RequestResponse/RequestResponse.csproj
dotnet run --project 04_Notification/Notification.csproj
```

## Key point: 01 vs 02

Projects 01 and 02 print the **same output**, but the design is different:

- **01**: every `User` holds a list of other users and delivers messages itself.
  Adding a user means editing many peer lists. Coupling grows as N-to-N.
- **02**: every `User` holds only a mediator. The mediator does the routing.
  Adding a user touches only the mediator.

Same behavior, very different coupling. That is the whole point of the pattern.

## 03 and 04: the "application" mediator

Libraries like MediatR use two message shapes:

- **Request/Response (03)** — one request goes to exactly **one** handler and
  returns a value. `mediator.Send(new Add(3, 4))` -> `7`.
- **Notification (04)** — one event goes to **many** handlers and returns
  nothing. `mediator.Publish(new OrderPlaced(...))` -> email + inventory + audit.

The `Mediator` classes here are tiny hand-rolled versions, so you can see
what a library like MediatR does internally without any dependency.
