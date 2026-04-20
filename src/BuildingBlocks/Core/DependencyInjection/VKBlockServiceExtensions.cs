using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace VK.Blocks.Core.DependencyInjection;

/// <summary>
/// Extension methods for managing building block service markers in an <see cref="IServiceCollection"/>.
/// </summary>
public static class VKBlockServiceExtensions
{
    /// <summary>
    /// Checks if a specific building block is already registered in the service collection.
    /// Following Rule 13 (Check-Self), this also automatically validates Rule 13 (Check-Prerequisite)
    /// recursively using the block marker's instance-based dependency tree.
    /// </summary>
    /// <remarks>
    /// <b>Zero-Reflection Architecture:</b>
    /// This method uses the <see cref="IVKBlockMarkerProvider{TSelf}.Instance"/> bridge to perform
    /// recursive validation without runtime reflection.
    /// </remarks>
    /// <typeparam name="TMarker">The marker type representing the building block.</typeparam>
    /// <param name="services">The service collection to check.</param>
    /// <returns><c>true</c> if the block is already registered; <c>false</c> if it is new and all dependencies are met.</returns>
    /// <exception cref="InvalidOperationException">Thrown if any required dependency is not registered.</exception>
    public static bool IsVKBlockRegistered<TMarker>(
        this IServiceCollection services)
        where TMarker : class, IVKBlockMarker, IVKBlockMarkerProvider<TMarker>
    {
        // Check for the identifier-based marker to ensure identity-level idempotency
        if (services.IsVKBlockRegistered(TMarker.Instance.Identifier))
        {
            return true;
        }

        // Automated recursive dependency validation via the static bridge interface
        IVKBlockMarker marker = TMarker.Instance;
        marker.EnsureDependenciesRegistered(services, marker.Identifier);

        return false;
    }

    /// <summary>
    /// Checks if a specific building block identifier is already registered.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="identifier">The building block identifier.</param>
    /// <returns><c>true</c> if registered; otherwise, <c>false</c>.</returns>
    public static bool IsVKBlockRegistered(this IServiceCollection services, string identifier)
        => services.Any(d => d.ServiceType == typeof(VKBlockRuntimeMarker)
                             && d.ImplementationInstance is VKBlockRuntimeMarker marker
                             && marker.Identifier == identifier);

    /// <summary>
    /// Checks if a specific service or options type is already registered.
    /// Generic variant.
    /// </summary>
    public static bool IsVKServiceRegistered<TService>(
        this IServiceCollection services)
        where TService : class
        => services.Any(d => d.ServiceType == typeof(TService));

    /// <summary>
    /// Checks if a specific service type is already registered.
    /// Non-generic variant for recursive validation.
    /// </summary>
    public static bool IsVKServiceRegistered(
        this IServiceCollection services,
        Type serviceType)
        => services.Any(d => d.ServiceType == serviceType);

    /// <summary>
    /// Registers a marker in the service collection to indicate that a building block has been initialized.
    /// </summary>
    /// <typeparam name="TMarker">The marker type representing the building block.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddVKBlockMarker<TMarker>(
        this IServiceCollection services)
        where TMarker : class, IVKBlockMarker, IVKBlockMarkerProvider<TMarker>
    {
        // Use the source-generated singleton instance to get the identifier
        string identifier = TMarker.Instance.Identifier;
        
        // Manual idempotency check for this specific identifier to allow multiple blocks to register
        if (!services.IsVKBlockRegistered(identifier))
        {
            services.AddSingleton(new VKBlockRuntimeMarker(identifier));
        }

        // Keep the concrete type registration for legacy compatibility and direct dependency resolution
        services.TryAddSingleton<TMarker>();
        return services;
    }
}
