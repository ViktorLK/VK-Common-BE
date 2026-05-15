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
    public async Task<VKResult<IEnumerable<string>>> SearchAsync(
        string query,
        IVKAIArgs? args = null,
        CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(VKResult.Success<IEnumerable<string>>([])).ConfigureAwait(false);
    }
}
