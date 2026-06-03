using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.SemanticKernel.Retrieval.Internal;

/// <summary>
/// A no-op implementation of the retrieval engine for the Semantic Kernel block.
/// </summary>
internal sealed class NoOpAISKRetrievalEngine : IVKRetrievalEngine
{
    public Task<VKResult<IReadOnlyList<VKRetrievalResult>>> SearchAsync(
        string query,
        VKRetrievalArgs? args = null,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<VKRetrievalResult> emptyList = [];
        return Task.FromResult(VKResult.Success(emptyList));
    }
}
