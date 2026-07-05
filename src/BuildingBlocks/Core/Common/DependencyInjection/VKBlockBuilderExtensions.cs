using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace VK.Blocks.Core;

/// <summary>
/// Extension methods for customizing building block registrations via <see cref="IVKBlockBuilder{TMarker}"/>.
/// </summary>
public static class VKBlockBuilderExtensions
{
    /// <summary>
    /// Overrides a scoped service registration within the building block.
    /// </summary>
    /// <typeparam name="TMarker">The marker type for the building block.</typeparam>
    /// <typeparam name="TService">The service type or interface to register.</typeparam>
    /// <typeparam name="TImplementation">The implementation type of the service.</typeparam>
    /// <param name="builder">The building block builder instance.</param>
    /// <returns>The builder instance for chaining.</returns>
    public static IVKBlockBuilder<TMarker> WithScoped<TMarker, TService, TImplementation>(
        this IVKBlockBuilder<TMarker> builder)
        where TMarker : class, IVKBlockMarker
        where TService : class
        where TImplementation : class, TService
    {
        VKGuard.NotNull(builder).Services.Replace(ServiceDescriptor.Scoped<TService, TImplementation>());
        return builder;
    }

    /// <summary>
    /// Overrides a singleton service registration within the building block.
    /// </summary>
    /// <typeparam name="TMarker">The marker type for the building block.</typeparam>
    /// <typeparam name="TService">The service type or interface to register.</typeparam>
    /// <typeparam name="TImplementation">The implementation type of the service.</typeparam>
    /// <param name="builder">The building block builder instance.</param>
    /// <returns>The builder instance for chaining.</returns>
    public static IVKBlockBuilder<TMarker> WithSingleton<TMarker, TService, TImplementation>(
        this IVKBlockBuilder<TMarker> builder)
        where TMarker : class, IVKBlockMarker
        where TService : class
        where TImplementation : class, TService
    {
        VKGuard.NotNull(builder).Services.Replace(ServiceDescriptor.Singleton<TService, TImplementation>());
        return builder;
    }

    /// <summary>
    /// Overrides a transient service registration within the building block.
    /// </summary>
    /// <typeparam name="TMarker">The marker type for the building block.</typeparam>
    /// <typeparam name="TService">The service type or interface to register.</typeparam>
    /// <typeparam name="TImplementation">The implementation type of the service.</typeparam>
    /// <param name="builder">The building block builder instance.</param>
    /// <returns>The builder instance for chaining.</returns>
    public static IVKBlockBuilder<TMarker> WithTransient<TMarker, TService, TImplementation>(
        this IVKBlockBuilder<TMarker> builder)
        where TMarker : class, IVKBlockMarker
        where TService : class
        where TImplementation : class, TService
    {
        VKGuard.NotNull(builder).Services.Replace(ServiceDescriptor.Transient<TService, TImplementation>());
        return builder;
    }

    /// <summary>
    /// Try to add an enumerable scoped service registration (idempotent addition).
    /// </summary>
    /// <typeparam name="TMarker">The marker type for the building block.</typeparam>
    /// <typeparam name="TService">The service type or interface to register.</typeparam>
    /// <typeparam name="TImplementation">The implementation type of the service.</typeparam>
    /// <param name="builder">The building block builder instance.</param>
    /// <returns>The builder instance for chaining.</returns>
    public static IVKBlockBuilder<TMarker> TryAddEnumerableScoped<TMarker, TService, TImplementation>(
        this IVKBlockBuilder<TMarker> builder)
        where TMarker : class, IVKBlockMarker
        where TService : class
        where TImplementation : class, TService
    {
        VKGuard.NotNull(builder).Services.TryAddEnumerable(ServiceDescriptor.Scoped<TService, TImplementation>());
        return builder;
    }

    /// <summary>
    /// Try to add an enumerable singleton service registration (idempotent addition).
    /// </summary>
    /// <typeparam name="TMarker">The marker type for the building block.</typeparam>
    /// <typeparam name="TService">The service type or interface to register.</typeparam>
    /// <typeparam name="TImplementation">The implementation type of the service.</typeparam>
    /// <param name="builder">The building block builder instance.</param>
    /// <returns>The builder instance for chaining.</returns>
    public static IVKBlockBuilder<TMarker> TryAddEnumerableSingleton<TMarker, TService, TImplementation>(
        this IVKBlockBuilder<TMarker> builder)
        where TMarker : class, IVKBlockMarker
        where TService : class
        where TImplementation : class, TService
    {
        VKGuard.NotNull(builder).Services.TryAddEnumerable(ServiceDescriptor.Singleton<TService, TImplementation>());
        return builder;
    }

    /// <summary>
    /// Try to add an enumerable transient service registration (idempotent addition).
    /// </summary>
    /// <typeparam name="TMarker">The marker type for the building block.</typeparam>
    /// <typeparam name="TService">The service type or interface to register.</typeparam>
    /// <typeparam name="TImplementation">The implementation type of the service.</typeparam>
    /// <param name="builder">The building block builder instance.</param>
    /// <returns>The builder instance for chaining.</returns>
    public static IVKBlockBuilder<TMarker> TryAddEnumerableTransient<TMarker, TService, TImplementation>(
        this IVKBlockBuilder<TMarker> builder)
        where TMarker : class, IVKBlockMarker
        where TService : class
        where TImplementation : class, TService
    {
        VKGuard.NotNull(builder).Services.TryAddEnumerable(ServiceDescriptor.Transient<TService, TImplementation>());
        return builder;
    }
}
