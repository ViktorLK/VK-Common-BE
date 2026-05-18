using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Memory.Internal;

/// <summary>
/// A no-op implementation of the graph memory layer.
/// </summary>
internal sealed class NullGraphMemory : IVKMemoryGraph
{
    public Task<VKResult> RelateAsync(
        string sourceId,
        string relationship,
        string targetId,
        IDictionary<string, object?>? properties = null,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(VKResult.Success());
    }

    public Task<VKResult<IEnumerable<string>>> GetRelatedAsync(
        string sourceId,
        string? relationship = null,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(VKResult.Success(Enumerable.Empty<string>()));
    }

    public Task<VKResult<IEnumerable<string>>> FindPathAsync(
        string startId,
        string endId,
        int maxDepth = 3,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(VKResult.Success(Enumerable.Empty<string>()));
    }
}
