using System.Reflection;
using MedTheMediator.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace MedTheMediator.Extensions;

public static class MedTheMediatorExtension
{
    public static IServiceCollection AddMedTheMediator(this IServiceCollection services, params Assembly[] assemblies)
    {
        services.AddTransient<IMediator, Mediator>();
        var handlerType = typeof(IHandler<,>);

        foreach (var assembly in assemblies)
        {
            var handlers = assembly.GetTypes()
                .Where(x => x is { IsAbstract: false, IsInterface: false })
                .SelectMany(x => x.GetInterfaces(), (t, i) => new { Type = t, Interface = i })
                .Where(ti => ti.Interface.IsGenericType &&
                             ti.Interface.GetGenericTypeDefinition() == handlerType);

            foreach (var handler in handlers)
            {
                services.AddTransient(handler.Interface, handler.Type);
            }
        }
        
        return services;
    }
}