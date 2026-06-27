using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.VectorSearch.Retrieval.Internal;

/// <summary>
/// Null search strategy returning empty results.
/// </summary>
internal sealed class NoOpSearchStrategy : IVKSearchStrategy
{
    public Task<VKResult<VKSearchResult[]>> SearchAsync(VKSearchQuery query, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(query);
        return Task.FromResult(VKResult.Success(System.Array.Empty<VKSearchResult>()));
    }
}
