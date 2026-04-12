using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace VK.Blocks.Authentication.Common.Extensions;

/// <summary>
/// Internal service collection extensions for registering in-memory cleanup infrastructure.
/// </summary>
internal static class InMemoryCleanupRegistration
{
    /// <summary>
    /// Registers a service that requires periodic in-memory cache cleanup.
    /// </summary>
    internal static void AddInMemoryCleanupProvider<TService, TImplementation>(this IServiceCollection services, ServiceLifetime lifetime)
        where TService : class
        where TImplementation : class, TService, IInMemoryCacheCleanup
    {
        // 0. Ensure identity-based idempotency to avoid duplicate factory registrations
        if (services.Any(d => d.ServiceType == typeof(TImplementation)))
        {
            return;
        }

        // 1. Register the concrete implementation as itself
        services.Add(new ServiceDescriptor(typeof(TImplementation), typeof(TImplementation), lifetime));

        // 2. Register the service interface as a factory resolving the concrete implementation
        services.Add(new ServiceDescriptor(typeof(TService), sp => sp.GetRequiredService<TImplementation>(), lifetime));

        // 3. Register the cleanup interface as a factory resolving the concrete implementation
        // Note: We use Add instead of TryAddEnumerable here because the guard check (Step 0) ensures
        // that we only register this specific implementation once in the cleanup collection.
        services.Add(new ServiceDescriptor(typeof(IInMemoryCacheCleanup), sp => sp.GetRequiredService<TImplementation>(), lifetime));

        // 4. Ensure the background service is registered once
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IHostedService, InMemoryCleanupBackgroundService>());
    }
}
