
# MedTheMediator

A minimal and extensible Mediator Pattern implementation for .NET application.

| Package | Version | Downloads |
| ------ | ------ | --------- |   
| `MedTheMediator` | [![NuGet](https://img.shields.io/nuget/v/MedTheMediator.svg)](https://nuget.org/packages/MedTheMediator) | [![downloads](https://img.shields.io/nuget/dt/MedTheMediator.svg)](https://nuget.org/packages/MedTheMediator)


## Installation

```bash
  dotnet add package MedTheMediator
```
    
## Examples

```csharp
public class Ping : IRequest<string>
{
    public string Message { get; set; }
}

public class PingHandler : IHandler<Ping, string>
{
    public async HandleAsync(Ping request, CancellationToken cancellationToken)
    {
        return Task.FromResult($"Pong: {request.Message}")
    }
}

// Register the handlers in DI container
services.AddMedTheMediator(typeof(PingHandler).Assembly);
```



## Resources
- Integration with ServiceCollection
- Automatic discovery and registration of handlers
- No external dependencies
