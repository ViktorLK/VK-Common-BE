using VK.Blocks.VectorIngest.Chunking.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;

namespace VK.Blocks.VectorIngest; // [AP.03] Internal namespace

/// <summary>
/// Configures and registers dependencies for the Chunking feature.
/// </summary>
[VKFeature(typeof(VKVectorIngestBlock), OptionsType = typeof(VKChunkingOptions))]
internal sealed partial class ChunkingFeature // [AP.01] sealed partial
{
    // [SG Hook]
    static partial void RegisterFeatureCustom(IServiceCollection services, VKChunkingOptions options)
    {
        _ = options;
        // [AP.02] TryAdd idempotent registration
        services.TryAddKeyedSingleton<IVKTextChunker, DefaultRecursiveChunker>(VKChunkerType.Recursive);
        services.TryAddKeyedScoped<IVKTextChunker, DefaultHierarchicalChunker>(VKChunkerType.Hierarchical);

        // Also register DefaultRecursiveChunker as default and self-registration for Hierarchical dependency
        services.TryAddSingleton<DefaultRecursiveChunker>();
        services.TryAddSingleton<IVKTextChunker, DefaultRecursiveChunker>();
    }

    // [SG Hook]
    static partial void ValidateFeatureCustom(VKChunkingOptions options, System.Collections.Generic.List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
