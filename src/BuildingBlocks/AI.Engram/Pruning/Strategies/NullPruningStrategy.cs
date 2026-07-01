using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram.Pruning.Strategies;

/// <summary>
/// Default pruning strategy that never prunes content.
/// </summary>
internal sealed class NullPruningStrategy : IVKPruningStrategy
{
    public Task<VKResult<bool>> ShouldPruneAsync(string content, double score, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(content);
        return Task.FromResult(VKResult.Success(false));
    }
}
