using System;
using VK.Blocks.AI.Corpus.Filtering.Internal;
using VK.Blocks.AI.Corpus.Gathering.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Corpus;

/// <summary>
/// Chained builder extension methods for configuring individual features within the AI.Corpus block.
/// Public API feature entry points following BB.01 / BB.06.
/// </summary>
public static class VKAICorpusBuilderExtensions
{
    /// <summary>
    /// Adds the Gathering feature to the AI.Corpus building block.
    /// </summary>
    public static IVKAICorpusBuilder AddVKGathering(
        this IVKAICorpusBuilder builder,
        Func<VKGatheringOptions, VKGatheringOptions>? transform = null)
    {
        VKGuard.NotNull(builder);
        GatheringFeature.Register(builder, transform);
        return builder;
    }

    /// <summary>
    /// Adds the Filtering feature to the AI.Corpus building block.
    /// </summary>
    public static IVKAICorpusBuilder AddVKFiltering(
        this IVKAICorpusBuilder builder,
        Func<VKFilteringOptions, VKFilteringOptions>? transform = null)
    {
        VKGuard.NotNull(builder);
        FilteringFeature.Register(builder, transform);
        return builder;
    }

    /// <summary>
    /// Adds the Tracking feature to the AI.Corpus building block.
    /// </summary>
    public static IVKAICorpusBuilder AddVKTracking(
        this IVKAICorpusBuilder builder,
        Func<VKTrackingOptions, VKTrackingOptions>? transform = null)
    {
        VKGuard.NotNull(builder);
        Tracking.Internal.TrackingFeature.Register(builder, transform);
        return builder;
    }

    /// <summary>
    /// Adds the Ingesting feature to the AI.Corpus building block.
    /// </summary>
    public static IVKAICorpusBuilder AddVKIngesting(
        this IVKAICorpusBuilder builder,
        Func<VKIngestingOptions, VKIngestingOptions>? transform = null)
    {
        VKGuard.NotNull(builder);
        Ingesting.Internal.IngestingFeature.Register(builder, transform);
        return builder;
    }

    /// <summary>
    /// Automatically enables all standard AI.Corpus features.
    /// </summary>
    public static IVKAICorpusBuilder AddVKDefaultFeatures(this IVKAICorpusBuilder builder)
    {
        VKGuard.NotNull(builder);
        return builder
            .AddVKGathering()
            .AddVKFiltering()
            .AddVKTracking()
            .AddVKIngesting();
    }
}
