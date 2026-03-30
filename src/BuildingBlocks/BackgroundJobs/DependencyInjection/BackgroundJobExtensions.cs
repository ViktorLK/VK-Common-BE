using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.BackgroundJobs.Abstractions;

namespace VK.Blocks.BackgroundJobs.DependencyInjection;

public static class BackgroundJobExtensions
{
    /// <summary>
    /// Scans the specified assembly and registers all classes that implement <see cref="IJobHandler{TData}"/> as scoped services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="assembly">The assembly to scan.</param>
    /// <returns>The modified service collection.</returns>
    public static IServiceCollection AddJobHandlersFromAssembly(this IServiceCollection services, Assembly assembly)
    {
        var handlerType = typeof(IJobHandler<>);
        
        var handlers = assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } &&
                        t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerType));

        foreach (var handler in handlers)
        {
            services.AddScoped(handler);
        }

        return services;
    }
}
