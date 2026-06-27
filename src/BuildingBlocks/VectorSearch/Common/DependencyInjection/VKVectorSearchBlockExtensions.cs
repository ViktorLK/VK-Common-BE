using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.VectorSearch.Common.DependencyInjection.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.VectorSearch;

/// <summary>
/// Service collection extensions for the AI Recall building block.
/// Public API entry point following BB.03.
/// </summary>
public static class VKVectorSearchBlockExtensions
{
    /// <summary>
    /// Adds the AI Recall building block services using configuration.
    /// </summary>
    public static IVKVectorSearchBuilder AddVKVectorSearchBlock(this IServiceCollection services, IConfiguration configuration)
    {
        VKGuard.NotNull(services);
        VKGuard.NotNull(configuration);
        return VectorSearchBlockRegistration.Register(services, configuration);
    }

    /// <summary>
    /// Adds the AI Recall building block services using a functional transformation.
    /// </summary>
    public static IVKVectorSearchBuilder AddVKVectorSearchBlock(
        this IServiceCollection services,
        IConfiguration configuration,
        Func<VKVectorSearchOptions, VKVectorSearchOptions> transform)
    {
        VKGuard.NotNull(services);
        VKGuard.NotNull(configuration);
        VKGuard.NotNull(transform);
        return VectorSearchBlockRegistration.Register(services, configuration, transform);
    }
}
