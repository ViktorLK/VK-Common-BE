using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.AI.VectorStore.DependencyInjection.Internal;

namespace VK.Blocks.AI.VectorStore;

/// <summary>
/// Service collection extensions for VK.Blocks.AI.VectorStore module.
/// Public API Wrapper following BB.03.1.
/// </summary>
[ExcludeFromCodeCoverage]
public static class VKAIVectorStoreBlockExtensions
{
    /// <summary>
    /// Adds the AI Vector Store building block to the service collection.
    /// [WRAPPER] pattern for IConfiguration-based registration.
    /// </summary>
    public static IVKAIVectorStoreBuilder AddVKAIVectorStoreBlock(
        this IServiceCollection services,
        IConfiguration configuration)
        => AIVectorStoreBlockRegistration.Register(services, configuration);

    /// <summary>
    /// Adds the AI Vector Store building block to the service collection with custom configuration.
    /// Following ADR-016: Supports immutable options (init) via 'with' expressions.
    /// </summary>
    public static IVKAIVectorStoreBuilder AddVKAIVectorStoreBlock(
        this IServiceCollection services,
        IConfiguration configuration,
        Func<VKAIVectorStoreOptions, VKAIVectorStoreOptions> transform)
        => AIVectorStoreBlockRegistration.Register(services, configuration, transform);
}
