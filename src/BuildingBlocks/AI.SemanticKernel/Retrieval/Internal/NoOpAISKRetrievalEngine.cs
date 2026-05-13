using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.SemanticKernel.Retrieval.Internal;

/// <summary>
/// A No-Op implementation of the retrieval engine used when the feature is disabled.
/// </summary>
internal sealed class NoOpAISKRetrievalEngine : IVKRetrievalEngine
{
    /// <inheritdoc />
    public Task<VKResult<IEnumerable<string>>> SearchAsync(
        string query,
        IVKAIArgs? args = null,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(VKResult.Failure<IEnumerable<string>>(VKRetrievalErrors.FeatureDisabled));
    }
}
