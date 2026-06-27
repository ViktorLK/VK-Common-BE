using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;
using VK.Blocks.VectorIngest;

namespace VK.Blocks.VectorIngest.Chunking.Internal; // [AP.03] Internal namespace

/// <summary>
/// Configures and registers dependencies for the Chunking feature.
/// </summary>
internal sealed partial class ChunkingFeature // [AP.01] sealed partial
{
    // [SG Hook]
    static partial void RegisterCustom(IServiceCollection services, VKChunkingOptions options)
    {
        _ = options;
        services.TryAddKeyedSingleton<IVKTextChunker, DefaultFixedSizeChunker>(VKChunkerType.FixedSize); // [AP.02] TryAdd idempotent registration
        services.TryAddKeyedSingleton<IVKTextChunker, DefaultRecursiveChunker>(VKChunkerType.Recursive);
        
        // Register Semantic and Hierarchical chunkers
        services.TryAddKeyedScoped<IVKTextChunker, DefaultSemanticChunker>(VKChunkerType.Semantic);
        services.TryAddKeyedScoped<IVKTextChunker, DefaultHierarchicalChunker>(VKChunkerType.Hierarchical);
        
        // Also register DefaultRecursiveChunker as default and self-registration for Hierarchical dependency
        services.TryAddSingleton<DefaultRecursiveChunker>();
        services.TryAddSingleton<IVKTextChunker, DefaultRecursiveChunker>();
    }

    // [SG Hook]
    static partial void ValidateCustom(VKChunkingOptions options, System.Collections.Generic.List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
