using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram.Decay.Strategies;

/// <summary>
/// Time-based decay strategy.
/// </summary>
internal sealed class TimeBasedDecayStrategy : IVKDecayStrategy
{
    public Task<VKResult<string>> DecayAsync(string content, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(content);
        // Boilerplate placeholder logic
        return Task.FromResult(VKResult.Success(content));
    }
}
