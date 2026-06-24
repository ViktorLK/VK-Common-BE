using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.VectorSearch.Expansion.Internal;

/// <summary>
/// Null context expansion strategy that returns results unchanged.
/// </summary>
internal sealed class NoOpContextExpansionStrategy : IVKContextExpansionStrategy
{
    public Task<VKResult<VKSearchResult[]>> ExpandContextAsync(VKSearchResult[] results, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(results);
        return Task.FromResult(VKResult.Success(results));
    }
}
