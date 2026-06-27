using System;
using VK.Blocks.VectorSearch.SemanticCache.Internal;
using VK.Blocks.VectorSearch.VectorReranking.Internal;
using VK.Blocks.VectorSearch.SearchGuard.Internal;
using VK.Blocks.VectorSearch.ContextExpansion.Internal;
using VK.Blocks.VectorSearch.ContextCompression.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.VectorSearch;

/// <summary>
/// Extension methods for chaining AI Recall features.
/// </summary>
public static class VKVectorSearchBuilderExtensions
{
    /// <summary>
    /// Adds the Semantic Cache feature to the AI Recall building block.
    /// </summary>
    public static IVKVectorSearchBuilder AddVKSemanticCache(
        this IVKVectorSearchBuilder builder,
        Func<VKSemanticCacheOptions, VKSemanticCacheOptions>? transform = null)
    {
        VKGuard.NotNull(builder); // [AP.01]
        SemanticCacheFeature.Register(builder, transform);
        return builder;
    }

    /// <summary>
    /// Adds the Vector Reranking feature to the AI Recall building block.
    /// </summary>
    public static IVKVectorSearchBuilder AddVKVectorReranking(
        this IVKVectorSearchBuilder builder,
        Func<VKVectorRerankingOptions, VKVectorRerankingOptions>? transform = null)
    {
        VKGuard.NotNull(builder); // [AP.01]
        VectorRerankingFeature.Register(builder, transform);
        return builder;
    }

    /// <summary>
    /// Adds the Search Guard feature to the AI Recall building block.
    /// </summary>
    public static IVKVectorSearchBuilder AddVKSearchGuard(
        this IVKVectorSearchBuilder builder,
        Func<VKSearchGuardOptions, VKSearchGuardOptions>? transform = null)
    {
        VKGuard.NotNull(builder); // [AP.01]
        SearchGuardFeature.Register(builder, transform);
        return builder;
    }

    /// <summary>
    /// Adds the Context Expansion feature to the AI Recall building block.
    /// </summary>
    public static IVKVectorSearchBuilder AddVKContextExpansion(
        this IVKVectorSearchBuilder builder,
        Func<VKContextExpansionOptions, VKContextExpansionOptions>? transform = null)
    {
        VKGuard.NotNull(builder); // [AP.01]
        ContextExpansionFeature.Register(builder, transform);
        return builder;
    }

    /// <summary>
    /// Adds the Context Compression feature to the AI Recall building block.
    /// </summary>
    public static IVKVectorSearchBuilder AddVKContextCompression(
        this IVKVectorSearchBuilder builder,
        Func<VKContextCompressionOptions, VKContextCompressionOptions>? transform = null)
    {
        VKGuard.NotNull(builder); // [AP.01]
        ContextCompressionFeature.Register(builder, transform);
        return builder;
    }
}
