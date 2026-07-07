using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram.Decay.Strategies;

/// <summary>
/// Default decay strategy that passes the content through unchanged.
/// </summary>
internal sealed class NullDecayStrategy : IVKDecayStrategy
{
    public Task<VKResult<string>> DecayAsync(string content, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(content);
        return Task.FromResult(VKResult.Success(content));
    }
}
