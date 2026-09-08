// A request that expects a response of type TResponse.
// (Marker interface - it just carries the response type.)
public interface IRequest<TResponse>;

/// <summary>
/// 
/// </summary>
/// <typeparam name="TRequest"></typeparam>
/// <typeparam name="TResponse"></typeparam>
public interface IRequestHandler<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    TResponse Handle(TRequest request);
}

// The mediator: send a request, get its response.
// The caller does not know which class actually handles it.
public interface IMediator
{
    TResponse Send<TResponse>(IRequest<TResponse> request);
}

// A minimal, hand-rolled mediator (this is roughly what MediatR does inside).
public sealed class Mediator : IMediator
{
    // Maps a request type -> a function that runs its one handler.
    private readonly Dictionary<Type, Func<object, object>> _handlers = [];

    // Register the handler for a specific request/response pair.
    public void Register<TRequest, TResponse>(IRequestHandler<TRequest, TResponse> handler)
        where TRequest : IRequest<TResponse>
    {
        _handlers[typeof(TRequest)] = request => handler.Handle((TRequest)request)!;
    }

    public TResponse Send<TResponse>(IRequest<TResponse> request)
    {
        // Look up the single handler for this request's runtime type.
        var handler = _handlers[request.GetType()];
        return (TResponse)handler(request);
    }
}
