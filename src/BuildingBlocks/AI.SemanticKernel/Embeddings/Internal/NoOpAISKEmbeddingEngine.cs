using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.SemanticKernel.Embeddings.Internal;

/// <summary>
/// A No-Op implementation of the embedding engine used when the feature is disabled.
/// </summary>
internal sealed class NoOpAISKEmbeddingEngine : IVKEmbeddingsEngine
{
    /// <inheritdoc />
    public Task<VKResult<VKEmbeddingsResponse>> GetEmbeddingsAsync(
        IEnumerable<string> inputs,
        VKEmbeddingsArgs? args = null,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(VKResult.Failure<VKEmbeddingsResponse>(VKEmbeddingsErrors.FeatureDisabled));
    }
}
