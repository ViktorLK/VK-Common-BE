using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram.Consolidation.Internal;

/// <summary>
/// Default consolidation strategy that merges content with newlines.
/// </summary>
internal sealed class NullConsolidationStrategy : IVKConsolidationStrategy
{
    public Task<VKResult<string>> ConsolidateAsync(string[] contents, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(contents);
        return Task.FromResult(VKResult.Success(string.Join("\n", contents)));
    }
}
