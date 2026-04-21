using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using VK.Blocks.Authentication.Common.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.Authentication.Common.Extensions;

/// <summary>
/// Internal service collection extensions for registering in-memory cleanup infrastructure.
/// </summary>
internal static class InMemoryCleanupRegistration
{
    /// <summary>
    /// Registers a service that requires periodic in-memory cache cleanup.
    /// </summary>
    /// <typeparam name="TService">The service interface type.</typeparam>
    /// <typeparam name="TImplementation">The concrete implementation type implementing <see cref="IInMemoryCacheCleanup"/>.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="lifetime">The service lifetime.</param>
    internal static void AddInMemoryCleanupProvider<TService, TImplementation>(this IServiceCollection services, ServiceLifetime lifetime)
        where TService : class
        where TImplementation : class, TService, IInMemoryCacheCleanup
    {
        // 0. Ensure identity-based idempotency to avoid duplicate factory registrations
        if (services.IsVKServiceRegistered<TImplementation>())
        {
            return;
        }

        // 1. Register the concrete implementation as itself
        services.TryAdd(ServiceDescriptor.Describe(typeof(TImplementation), typeof(TImplementation), lifetime));

        // 2. Register the service interface as a factory resolving the concrete implementation
        services.TryAdd(ServiceDescriptor.Describe(typeof(TService), sp => sp.GetRequiredService<TImplementation>(), lifetime));

        // 3. Register the cleanup interface as the concrete implementation itself.
        // By and using the implementation type directly (instead of a lambda), TryAddEnumerable
        // can correctly detect duplicates (Rule 13). Since TImplementation is already registered
        // as itself, the DI container will resolve the same instance.
        services.TryAddEnumerable(ServiceDescriptor.Describe(typeof(IInMemoryCacheCleanup), typeof(TImplementation), lifetime));

        // 4. Ensure the background service is registered once
        services.TryAddEnumerableSingleton<IHostedService, InMemoryCleanupBackgroundService>();
    }
}







