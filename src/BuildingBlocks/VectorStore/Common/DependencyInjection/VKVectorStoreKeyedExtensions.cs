using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;

namespace VK.Blocks.VectorStore;

/// <summary>
/// Service collection extensions for registering Keyed <see cref="IVKEmbeddingsEngine"/> implementations (.NET 8+ Keyed Services).
/// Follows AP.01, AP.02, AP.03.
/// </summary>
public static class VKVectorStoreKeyedExtensions
{
    /// <summary>
    /// Registers a keyed <see cref="IVKEmbeddingsEngine"/> implementation for a specific service key (enum, string, etc.).
    /// </summary>
    public static IServiceCollection AddVKKeyedEmbeddingsEngine<TEngine>(
        this IServiceCollection services,
        object serviceKey,
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TEngine : class, IVKEmbeddingsEngine
    {
        VKGuard.NotNull(services);
        VKGuard.NotNull(serviceKey);

        services.TryAdd(ServiceDescriptor.DescribeKeyed(typeof(IVKEmbeddingsEngine), serviceKey, typeof(TEngine), lifetime));
        return services;
    }

    /// <summary>
    /// Registers a keyed <see cref="IVKEmbeddingsEngine"/> implementation for both enum and string representations of a key.
    /// </summary>
    public static IServiceCollection AddVKKeyedEmbeddingsEngine<TEnum, TEngine>(
        this IServiceCollection services,
        TEnum enumKey,
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TEnum : struct, Enum
        where TEngine : class, IVKEmbeddingsEngine
    {
        VKGuard.NotNull(services);

        services.TryAdd(ServiceDescriptor.DescribeKeyed(typeof(IVKEmbeddingsEngine), enumKey, typeof(TEngine), lifetime));
        services.TryAdd(ServiceDescriptor.DescribeKeyed(typeof(IVKEmbeddingsEngine), enumKey.ToString(), typeof(TEngine), lifetime));
        return services;
    }
}
