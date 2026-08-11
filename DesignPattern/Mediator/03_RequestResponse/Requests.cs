// Two independent request/handler pairs. Notice they know nothing about
// each other, and the caller knows only the request types.

// Request 1: ask for a greeting string.
public sealed record Greet(string Name) : IRequest<string>;

public sealed class GreetHandler : IRequestHandler<Greet, string>
{
    public string Handle(Greet request) => $"Hello, {request.Name}!";
}

// Request 2: ask for the sum of two numbers.
public sealed record Add(int A, int B) : IRequest<int>;

public sealed class AddHandler : IRequestHandler<Add, int>
{
    public int Handle(Add request) => request.A + request.B;
}
