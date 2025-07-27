using MedTheMediator.Abstractions;

namespace MedTheMediator.Tests.Requests;

public record Ping(string Message) : IRequest<string>;