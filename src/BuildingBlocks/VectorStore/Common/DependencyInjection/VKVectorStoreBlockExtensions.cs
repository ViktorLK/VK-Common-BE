using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.VectorStore.Common.DependencyInjection.Internal;

namespace VK.Blocks.VectorStore;

/// <summary>
/// Service collection extensions for VK.Blocks.VectorStore module.
/// Public API Wrapper following BB.03.1.
/// </summary>
[ExcludeFromCodeCoverage]
public static class VKVectorStoreBlockExtensions
{
    /// <summary>
    /// Adds the AI Vector Store building block to the service collection.
    /// [WRAPPER] pattern for IConfiguration-based registration.
    /// </summary>
    public static IVKVectorStoreBuilder AddVKVectorStoreBlock(
        this IServiceCollection services,
        IConfiguration configuration)
        => VectorStoreBlockRegistration.Register(services, configuration);

    /// <summary>
    /// Adds the AI Vector Store building block to the service collection with custom configuration.
    /// Following ADR-016: Supports immutable options (init) via 'with' expressions.
    /// </summary>
    public static IVKVectorStoreBuilder AddVKVectorStoreBlock(
        this IServiceCollection services,
        IConfiguration configuration,
        Func<VKVectorStoreOptions, VKVectorStoreOptions> transform)
        => VectorStoreBlockRegistration.Register(services, configuration, transform);
}
