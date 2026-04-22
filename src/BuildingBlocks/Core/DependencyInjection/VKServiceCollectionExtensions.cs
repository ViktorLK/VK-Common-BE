using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace VK.Blocks.Core;

/// <summary>
/// General-purpose extension methods for <see cref="IServiceCollection"/> to simplify dependency injection tasks.
/// These are not specific to building block logic.
/// </summary>
public static class VKServiceCollectionExtensions
{
    /// <summary>
    /// Try to add an enumerable scoped service registration (idempotent addition).
    /// </summary>
    /// <typeparam name="TService">The service type or interface to register.</typeparam>
    /// <typeparam name="TImplementation">The implementation type of the service.</typeparam>
    /// <param name="services">The service collection instance.</param>
    /// <returns>The service collection instance for chaining.</returns>
    public static IServiceCollection TryAddEnumerableScoped<TService, TImplementation>(this IServiceCollection services)
        where TService : class
        where TImplementation : class, TService
    {
        VKGuard.NotNull(services).TryAddEnumerable(ServiceDescriptor.Scoped<TService, TImplementation>());
        return services;
    }

    /// <summary>
    /// Try to add an enumerable singleton service registration (idempotent addition).
    /// </summary>
    /// <typeparam name="TService">The service type or interface to register.</typeparam>
    /// <typeparam name="TImplementation">The implementation type of the service.</typeparam>
    /// <param name="services">The service collection instance.</param>
    /// <returns>The service collection instance for chaining.</returns>
    public static IServiceCollection TryAddEnumerableSingleton<TService, TImplementation>(this IServiceCollection services)
        where TService : class
        where TImplementation : class, TService
    {
        VKGuard.NotNull(services).TryAddEnumerable(ServiceDescriptor.Singleton<TService, TImplementation>());
        return services;
    }

    /// <summary>
    /// Try to add an enumerable transient service registration (idempotent addition).
    /// </summary>
    /// <typeparam name="TService">The service type or interface to register.</typeparam>
    /// <typeparam name="TImplementation">The implementation type of the service.</typeparam>
    /// <param name="services">The service collection instance.</param>
    /// <returns>The service collection instance for chaining.</returns>
    public static IServiceCollection TryAddEnumerableTransient<TService, TImplementation>(this IServiceCollection services)
        where TService : class
        where TImplementation : class, TService
    {
        VKGuard.NotNull(services).TryAddEnumerable(ServiceDescriptor.Transient<TService, TImplementation>());
        return services;
    }
}
