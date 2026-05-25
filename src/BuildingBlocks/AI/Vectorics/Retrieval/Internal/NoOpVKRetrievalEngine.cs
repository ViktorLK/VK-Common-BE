using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Vectorics.Retrieval.Internal;

/// <summary>
/// No-op implementation of <see cref="IVKRetrievalEngine"/>.
/// Returns an empty result set.
/// </summary>
internal sealed class NoOpVKRetrievalEngine : IVKRetrievalEngine
{
    // [SG Hook]
    public Task<VKResult<IReadOnlyList<VKRetrievalResult>>> SearchAsync(
        string query,
        VKRetrievalArgs? args = null,
        CancellationToken cancellationToken = default)
    {
        _ = query;
        _ = args;
        _ = cancellationToken;

        IReadOnlyList<VKRetrievalResult> emptyList = [];
        return Task.FromResult(VKResult.Success(emptyList));
    }
}
