using System;
using VK.Blocks.AI.Corpus.Filtering.Internal;
using VK.Blocks.AI.Corpus.KnowledgeSourcing.Internal;
using VK.Blocks.AI.Corpus.Tracking.Internal;
using VK.Blocks.AI.Corpus.CorpusTracking.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Corpus;

/// <summary>
/// Chained builder extension methods for configuring individual features within the AI.Corpus block.
/// Public API feature entry points following BB.01 / BB.06.
/// </summary>
public static class VKCorpusBuilderExtensions
{
    /// <summary>
    /// Adds the Knowledge Sourcing feature to the AI.Corpus building block.
    /// </summary>
    public static IVKCorpusBuilder AddVKKnowledgeSourcing(
        this IVKCorpusBuilder builder,
        Func<VKKnowledgeSourcingOptions, VKKnowledgeSourcingOptions>? transform = null)
    {
        VKGuard.NotNull(builder);
        KnowledgeSourcingFeature.Register(builder, transform);
        return builder;
    }

    /// <summary>
    /// Adds the Filtering feature to the AI.Corpus building block.
    /// </summary>
    public static IVKCorpusBuilder AddVKFiltering(
        this IVKCorpusBuilder builder,
        Func<VKFilteringOptions, VKFilteringOptions>? transform = null)
    {
        VKGuard.NotNull(builder);
        FilteringFeature.Register(builder, transform);
        return builder;
    }

    /// <summary>
    /// Adds the Tracking feature to the AI.Corpus building block.
    /// </summary>
    public static IVKCorpusBuilder AddVKTracking(
        this IVKCorpusBuilder builder,
        Func<VKCorpusTrackingOptions, VKCorpusTrackingOptions>? transform = null)
    {
        VKGuard.NotNull(builder);
        CorpusTrackingFeature.Register(builder, transform);
        return builder;
    }

    /// <summary>
    /// Automatically enables all standard AI.Corpus features.
    /// </summary>
    public static IVKCorpusBuilder AddVKDefaultFeatures(this IVKCorpusBuilder builder)
    {
        VKGuard.NotNull(builder);
        return builder
            .AddVKKnowledgeSourcing()
            .AddVKFiltering()
            .AddVKTracking();
    }
}
