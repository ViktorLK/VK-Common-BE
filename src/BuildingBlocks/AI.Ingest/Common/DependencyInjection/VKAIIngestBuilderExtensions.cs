using System;
using VK.Blocks.AI.Ingest.Pipelines.Internal;
using VK.Blocks.AI.Ingest.VecLoader.Internal;
using VK.Blocks.AI.Ingest.VecSink.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Ingest;

/// <summary>
/// Extension methods for chaining AI Ingest features.
/// </summary>
public static class VKAIIngestBuilderExtensions
{
    /// <summary>
    /// Adds the VecLoader feature to the AI Ingest building block.
    /// </summary>
    public static IVKAIIngestBuilder AddVKVecLoader(
        this IVKAIIngestBuilder builder,
        Func<VKVecLoaderOptions, VKVecLoaderOptions>? transform = null)
    {
        VKGuard.NotNull(builder);
        VecLoaderFeature.Register(builder, transform);
        return builder;
    }

    /// <summary>
    /// Adds the VecSink feature to the AI Ingest building block.
    /// </summary>
    public static IVKAIIngestBuilder AddVKVecSink(
        this IVKAIIngestBuilder builder,
        Func<VKVecSinkOptions, VKVecSinkOptions>? transform = null)
    {
        VKGuard.NotNull(builder);
        VecSinkFeature.Register(builder, transform);
        return builder;
    }

    /// <summary>
    /// Adds the Pipelines feature to the AI Ingest building block.
    /// </summary>
    public static IVKAIIngestBuilder AddVKPipelines(
        this IVKAIIngestBuilder builder,
        Func<VKPipelinesOptions, VKPipelinesOptions>? transform = null)
    {
        VKGuard.NotNull(builder);
        PipelinesFeature.Register(builder, transform);
        return builder;
    }

    /// <summary>
    /// Automatically enables all standard AI Ingest features.
    /// </summary>
    public static IVKAIIngestBuilder AddVKDefaultFeatures(this IVKAIIngestBuilder builder)
    {
        VKGuard.NotNull(builder);
        return builder
            .AddVKVecLoader()
            .AddVKVecSink()
            .AddVKPipelines();
    }
}
