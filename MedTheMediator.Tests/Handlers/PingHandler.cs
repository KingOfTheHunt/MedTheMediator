using MedTheMediator.Abstractions;
using MedTheMediator.Tests.Requests;

namespace MedTheMediator.Tests.Handlers;

public class PingHandler : IHandler<Ping, string>
{
    public Task<string> HandleAsync(Ping request, CancellationToken cancellationToken = new CancellationToken())
    {
        return Task.FromResult($"Pong: {request.Message}");
    }
}