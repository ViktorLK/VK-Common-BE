using System;
using VK.Blocks.VectorIngest.Pipeline.Internal;
using VK.Blocks.VectorIngest.DocumentLoader.Internal;
using VK.Blocks.VectorIngest.Parsing.Internal;
using VK.Blocks.VectorIngest.Chunking.Internal;
using VK.Blocks.VectorIngest.Enrichment.Internal;
using VK.Blocks.VectorIngest.Deduplication.Internal;
using VK.Blocks.VectorIngest.Indexing.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.VectorIngest;


/// <summary>
/// Extension methods for chaining AI Ingest features.
/// </summary>
public static class VKVectorIngestBuilderExtensions
{
    /// <summary>
    /// Adds the DocumentLoader feature to the AI Ingest building block.
    /// </summary>
    public static IVKVectorIngestBuilder AddVKDocumentLoader(
        this IVKVectorIngestBuilder builder,
        Func<VKDocumentLoaderOptions, VKDocumentLoaderOptions>? transform = null)
    {
        VKGuard.NotNull(builder);
        DocumentLoaderFeature.Register(builder, transform);
        return builder;
    }


    /// <summary>
    /// Adds the Pipeline feature to the AI Ingest building block.
    /// </summary>
    public static IVKVectorIngestBuilder AddVKPipeline(
        this IVKVectorIngestBuilder builder,
        Func<VKPipelineOptions, VKPipelineOptions>? transform = null)
    {
        VKGuard.NotNull(builder);
        PipelineFeature.Register(builder, transform);
        return builder;
    }

    /// <summary>
    /// Adds the Parsing feature to the AI Ingest building block.
    /// </summary>
    public static IVKVectorIngestBuilder AddVKParsing(
        this IVKVectorIngestBuilder builder,
        Func<VKParsingOptions, VKParsingOptions>? transform = null)
    {
        VKGuard.NotNull(builder);
        ParsingFeature.Register(builder, transform);
        return builder;
    }

    /// <summary>
    /// Adds the Chunking feature to the AI Ingest building block.
    /// </summary>
    public static IVKVectorIngestBuilder AddVKChunking(
        this IVKVectorIngestBuilder builder,
        Func<VKChunkingOptions, VKChunkingOptions>? transform = null)
    {
        VKGuard.NotNull(builder);
        ChunkingFeature.Register(builder, transform);
        return builder;
    }

    /// <summary>
    /// Adds the Enrichment feature to the AI Ingest building block.
    /// </summary>
    public static IVKVectorIngestBuilder AddVKEnrichment(
        this IVKVectorIngestBuilder builder,
        Func<VKEnrichmentOptions, VKEnrichmentOptions>? transform = null)
    {
        VKGuard.NotNull(builder);
        EnrichmentFeature.Register(builder, transform);
        return builder;
    }

    /// <summary>
    /// Adds the Deduplication feature to the AI Ingest building block.
    /// </summary>
    public static IVKVectorIngestBuilder AddVKDeduplication(
        this IVKVectorIngestBuilder builder,
        Func<VKDeduplicationOptions, VKDeduplicationOptions>? transform = null)
    {
        VKGuard.NotNull(builder);
        DeduplicationFeature.Register(builder, transform);
        return builder;
    }


    /// <summary>
    /// Adds the Indexing feature to the AI Ingest building block.
    /// </summary>
    public static IVKVectorIngestBuilder AddVKIndexing(
        this IVKVectorIngestBuilder builder,
        Func<VKIndexingOptions, VKIndexingOptions>? transform = null)
    {
        VKGuard.NotNull(builder);
        IndexingFeature.Register(builder, transform);
        return builder;
    }

    /// <summary>
    /// Automatically enables all standard AI Ingest features.
    /// </summary>
    public static IVKVectorIngestBuilder AddVKDefaultFeatures(this IVKVectorIngestBuilder builder)
    {
        VKGuard.NotNull(builder);
        return builder
            .AddVKParsing()
            .AddVKChunking()
            .AddVKEnrichment()
            .AddVKIndexing()
            .AddVKDocumentLoader()
            .AddVKPipeline();
    }
}
