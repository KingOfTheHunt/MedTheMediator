using System.Collections.Concurrent;
using System.Linq.Expressions;
using MedTheMediator.Abstractions;
using MedTheMediator.Exceptions;
using MedTheMediator.Utils;

namespace MedTheMediator;

public class Mediator(IServiceProvider serviceProvider) : IMediator
{
    private static readonly ConcurrentDictionary<(Type, Type), Func<object, object, CancellationToken, Task<object>>>
        HandlersCache = new();
    
    /// <summary>Send a request to the appropriate handler (<see cref="IHandler{TRequest, TResponse}"/>)
    /// and returns the response.
    /// </summary>
    /// <typeparam name="TRequest">Type of request being sent.</typeparam>
    /// <typeparam name="TResponse">Type of response returned.</typeparam>
    /// <param name="request">The request instance to be processed.</param>
    /// <param name="cancellationToken">Optional token to cancel the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation, containing the response of
    /// type <typeparamref name="TResponse"></typeparamref>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is null.</exception>
    /// <exception cref="HandlerNotFoundException">Thrown when an appropriate handler is not found.</exception>
    /// <exception cref="MethodNotFoundException">Thrown when the <c>HandleAsync</c> method is not found
    /// on the handler.</exception>
    /// <remarks>
    /// Uses an internal cache to store compiled delegates for invoking <c>HandleAsync</c>, avoiding
    /// the overhead of reflection on subsequent calls.
    /// </remarks>
    public async Task<TResponse> SendAsync<TRequest, TResponse>(
        TRequest request, 
        CancellationToken cancellationToken = default)
    {
        if (request is null)
            throw new ArgumentNullException(nameof(request));

        var requestType = request.GetType();
        var responseType = typeof(TResponse);
        var handlerType = typeof(IHandler<,>).MakeGenericType(requestType, responseType);
        var handler = serviceProvider.GetService(handlerType)
                      ?? throw new HandlerNotFoundException($"Handler of type {handlerType} not found.");

        var key = (requestType, responseType);
        var executor = HandlersCache.GetOrAdd(key, static tuple =>
        {
            var (reqType, resType) = tuple;
            var handler = typeof(IHandler<,>).MakeGenericType(reqType, resType);
            var method = handler.GetMethod("HandleAsync") ??
                         throw new MethodNotFoundException("Method HandleAsync was not found.");

            // Adicionando os parâmetros para o delegate
            var handlerParameter = Expression.Parameter(typeof(object), "handler");
            var requestParameter = Expression.Parameter(typeof(object), "request");
            var cancellationTokenParameter = Expression.Parameter(typeof(CancellationToken), "cancellationToken");

            // Converter os parâmetros para o seu tipo específico.
            var castedHandler = Expression.Convert(handlerParameter, handler);
            var castedRequest = Expression.Convert(requestParameter, reqType);

            // Definindo a assinatura do método
            var call = Expression.Call(castedHandler, method, castedRequest, cancellationTokenParameter);

            // Convertendo o tipo de retorno para object
            var castedMethod = typeof(TaskCaster)
                .GetMethod(nameof(TaskCaster.Cast))!
                .MakeGenericMethod(resType);
            var castedCall = Expression.Call(castedMethod, call);

            var lambda = Expression.Lambda<Func<object, object, CancellationToken, Task<object>>>(
                castedCall, handlerParameter, requestParameter, cancellationTokenParameter);

            return lambda.Compile();
        });

        var result = await executor(handler, request, cancellationToken);

        return (TResponse)result;
    }
}