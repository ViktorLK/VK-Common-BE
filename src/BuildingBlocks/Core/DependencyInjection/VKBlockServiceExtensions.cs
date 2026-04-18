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
    /// </summary>
    /// <typeparam name="TMarker">The marker type representing the building block (usually the block class itself).</typeparam>
    /// <param name="services">The service collection to check.</param>
    /// <returns><c>true</c> if the block is registered; otherwise, <c>false</c>.</returns>
    public static bool IsVKBlockRegistered<TMarker>(
        this IServiceCollection services)
        where TMarker : class
        => services.Any(d => d.ServiceType == typeof(TMarker));

    /// <summary>
    /// Registers a marker in the service collection to indicate that a building block has been initialized.
    /// </summary>
    /// <typeparam name="TMarker">The marker type representing the building block.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddVKBlockMarker<TMarker>(
        this IServiceCollection services)
        where TMarker : class
    {
        services.TryAddSingleton<TMarker>();
        return services;
    }
}



