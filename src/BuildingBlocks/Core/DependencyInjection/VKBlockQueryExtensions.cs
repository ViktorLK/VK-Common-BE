using System;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Core.DependencyInjection.Internal;

namespace VK.Blocks.Core;

/// <summary>
/// Extension methods for querying building block registrations and services in an <see cref="IServiceCollection"/>.
/// These methods provide read-only diagnostic and validation capabilities.
/// </summary>
public static class VKBlockQueryExtensions
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
        VKGuard.NotNull(services);

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
    {
        VKGuard.NotNull(services);
        VKGuard.NotNull(identifier);

        for (int i = 0; i < services.Count; i++)
        {
            ServiceDescriptor descriptor = services[i];
            if (descriptor.ServiceType == typeof(BlockRuntimeMarker) &&
                descriptor.ImplementationInstance is BlockRuntimeMarker marker &&
                marker.Identifier == identifier)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Checks if a specific service or options type is already registered.
    /// Generic variant.
    /// </summary>
    /// <typeparam name="TService">The service type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns><c>true</c> if registered; otherwise, <c>false</c>.</returns>
    public static bool IsVKServiceRegistered<TService>(
        this IServiceCollection services)
        where TService : class
    {
        VKGuard.NotNull(services);

        for (int i = 0; i < services.Count; i++)
        {
            if (services[i].ServiceType == typeof(TService))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Checks if a specific service type is already registered.
    /// Non-generic variant for recursive validation.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="serviceType">The type of the service.</param>
    /// <returns><c>true</c> if registered; otherwise, <c>false</c>.</returns>
    public static bool IsVKServiceRegistered(
        this IServiceCollection services,
        Type serviceType)
    {
        VKGuard.NotNull(services);
        VKGuard.NotNull(serviceType);

        for (int i = 0; i < services.Count; i++)
        {
            if (services[i].ServiceType == serviceType)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Attempts to retrieve a registered singleton instance of a service from the collection.
    /// Only works if the service was registered with a concrete instance (ImplementationInstance).
    /// </summary>
    /// <typeparam name="TService">The service type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The instance if found; otherwise, <c>null</c>.</returns>
    public static TService? GetVKServiceInstance<TService>(this IServiceCollection services)
        where TService : class
    {
        VKGuard.NotNull(services);

        for (int i = 0; i < services.Count; i++)
        {
            ServiceDescriptor descriptor = services[i];
            if (descriptor.ServiceType == typeof(TService) &&
                descriptor.ImplementationInstance is TService instance)
            {
                return instance;
            }
        }

        return null;
    }
}
