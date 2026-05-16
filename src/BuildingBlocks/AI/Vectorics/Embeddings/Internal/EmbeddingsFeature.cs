using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace VK.Blocks.AI.Vectorics.Embeddings.Internal;

/// <summary>
/// Embeddings feature marker and registration hub.
/// </summary>
internal sealed partial class EmbeddingsFeature
{
    // [SG Hook]
    static partial void RegisterCustom(IServiceCollection services, VKEmbeddingsOptions options)
    {
        _ = options;
        services.TryAddSingleton<IVKEmbeddingsEngine, NoOpVKEmbeddingsEngine>();
    }

    /// <summary>Add embeddings-specific validation logic here</summary>
    // [SG Hook]
    static partial void ValidateCustom(VKEmbeddingsOptions options, List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
