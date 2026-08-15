using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Service collection extensions for registering Keyed AI Engines (.NET 8+ Keyed Services).
/// Follows AP.01, AP.02, AP.03.
/// </summary>
public static class VKAIKeyedEngineExtensions
{
    /// <summary>
    /// Registers a keyed <see cref="IVKChatEngine"/> implementation for a specific <see cref="VKAIProviderType"/>.
    /// </summary>
    public static IServiceCollection AddVKKeyedChatEngine<TEngine>(
        this IServiceCollection services,
        VKAIProviderType providerType,
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TEngine : class, IVKChatEngine
    {
        VKGuard.NotNull(services);

        services.TryAddKeyed(typeof(IVKChatEngine), providerType, typeof(TEngine), lifetime);
        services.TryAddKeyed(typeof(IVKChatEngine), providerType.ToString(), typeof(TEngine), lifetime);
        return services;
    }

    /// <summary>
    /// Registers a keyed <see cref="IVKChatEngine"/> implementation for a custom string provider key.
    /// </summary>
    public static IServiceCollection AddVKKeyedChatEngine<TEngine>(
        this IServiceCollection services,
        string providerKey,
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TEngine : class, IVKChatEngine
    {
        VKGuard.NotNull(services);
        VKGuard.NotNullOrWhiteSpace(providerKey);

        services.TryAddKeyed(typeof(IVKChatEngine), providerKey, typeof(TEngine), lifetime);
        return services;
    }

    /// <summary>
    /// Registers a keyed <see cref="IVKTextEngine"/> implementation for a specific <see cref="VKAIProviderType"/>.
    /// </summary>
    public static IServiceCollection AddVKKeyedTextEngine<TEngine>(
        this IServiceCollection services,
        VKAIProviderType providerType,
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TEngine : class, IVKTextEngine
    {
        VKGuard.NotNull(services);

        services.TryAddKeyed(typeof(IVKTextEngine), providerType, typeof(TEngine), lifetime);
        services.TryAddKeyed(typeof(IVKTextEngine), providerType.ToString(), typeof(TEngine), lifetime);
        return services;
    }

    /// <summary>
    /// Registers a keyed <see cref="IVKImageGenerationEngine"/> implementation for a specific <see cref="VKAIProviderType"/>.
    /// </summary>
    public static IServiceCollection AddVKKeyedImageGenerationEngine<TEngine>(
        this IServiceCollection services,
        VKAIProviderType providerType,
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TEngine : class, IVKImageGenerationEngine
    {
        VKGuard.NotNull(services);

        services.TryAddKeyed(typeof(IVKImageGenerationEngine), providerType, typeof(TEngine), lifetime);
        services.TryAddKeyed(typeof(IVKImageGenerationEngine), providerType.ToString(), typeof(TEngine), lifetime);
        return services;
    }

    private static void TryAddKeyed(
        this IServiceCollection services,
        Type serviceType,
        object serviceKey,
        Type implementationType,
        ServiceLifetime lifetime)
    {
        var descriptor = ServiceDescriptor.DescribeKeyed(serviceType, serviceKey, implementationType, lifetime);
        services.TryAdd(descriptor);
    }
}
