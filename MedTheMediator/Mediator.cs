using MedTheMediator.Abstractions;
using MedTheMediator.Exceptions;

namespace MedTheMediator;

public class Mediator(IServiceProvider serviceProvider) : IMediator
{
    public Task<TResponse> SendAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = new CancellationToken())
    {
        if (request is null)
            throw new ArgumentNullException(nameof(request));
        
        var requestType = request.GetType();
        var handlerType = typeof(IHandler<,>).MakeGenericType(requestType, typeof(TResponse));
        var handler = serviceProvider.GetService(handlerType);

        if (handler is null)
            throw new HandlerNotFoundException($"Handler of type {handlerType} not found");
        
        var method = handlerType.GetMethod("HandleAsync");
        
        if (method is null)
            throw new MethodNotFoundException("Method HandleAsync not found");
        
        var result = method.Invoke(handler, [request, cancellationToken]);
        
        if (result is not Task<TResponse> response)
            throw new ResponseException($"Response is different than {typeof(TResponse)}");
        
        return response;
    }
}