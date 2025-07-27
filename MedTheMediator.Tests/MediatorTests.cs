using MedTheMediator.Abstractions;
using MedTheMediator.Exceptions;
using MedTheMediator.Extensions;
using MedTheMediator.Tests.Handlers;
using MedTheMediator.Tests.Requests;
using Microsoft.Extensions.DependencyInjection;

namespace MedTheMediator.Tests;

[TestClass]
public class MediatorTests
{
    [TestMethod]
    public async Task ShouldReturnResponseWhenHandlerIsRegistered()
    {
        var mediator = CreateMediatorWithHandlers();
        var response = await mediator.SendAsync<Ping, string>(new Ping("Hello"));
        Assert.AreEqual("Pong: Hello", response);
    }

    [TestMethod]
    public async Task ShouldThrowHandlerNotFoundExceptionWhenHandlerIsNotRegistered()
    {
        var mediator = CreateMediatorWithoutHandlers();

        await Assert.ThrowsExceptionAsync<HandlerNotFoundException>(async () =>
        {
            await mediator.SendAsync<Ping, string>(new Ping("Hello"));
        });
    }

    [TestMethod]
    public async Task ShouldThrowArgumentNullExceptionWhenRequestIsNull()
    {
        var mediator = CreateMediatorWithHandlers();

        await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
        {
            await mediator.SendAsync<Ping, string>(null!);
        });
    }

    private IMediator CreateMediatorWithHandlers()
    {
        var services = new ServiceCollection();
        services.AddMedTheMediator(typeof(PingHandler).Assembly);
        return services.BuildServiceProvider().GetRequiredService<IMediator>();
    }

    private IMediator CreateMediatorWithoutHandlers()
    {
        var services = new ServiceCollection();
        services.AddMedTheMediator();
        return services.BuildServiceProvider().GetRequiredService<IMediator>();
    }
}