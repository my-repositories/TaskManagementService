using System.Reflection;
using TaskManagementService.Domain.Interfaces;

namespace TaskManagementService.Listener.Rabbit.Api.Extensions;

public static class HandlerExtensions
{
    public static IServiceCollection AddEventHandlers(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        var handlerTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(ITaskEventHandler).IsAssignableFrom(t));

        foreach (var type in handlerTypes)
        {
            services.AddScoped(typeof(ITaskEventHandler), type);
        }

        return services;
    }
}
