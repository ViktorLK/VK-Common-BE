using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;
using VK.Blocks.VectorStore;

namespace VK.Blocks.AI.SemanticKernel.Embeddings.Internal;

/// <summary>
/// A No-Op implementation of the embedding engine used when the feature is disabled.
/// </summary>
internal sealed class NoOpAISKEmbeddingEngine : IVKEmbeddingsEngine
{
    /// <inheritdoc />
    public Task<VKResult<VKVector>> GenerateAsync(
        string text,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(VKResult.Failure<VKVector>(VKError.Failure("AI.Embeddings.FeatureDisabled", "The embeddings feature is disabled.")));
    }
}
