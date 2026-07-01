using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram.Pruning.Strategies;

/// <summary>
/// Pruning strategy that prunes content below a specific score threshold.
/// </summary>
internal sealed class ThresholdPruningStrategy : IVKPruningStrategy
{
    private readonly VKPruningOptions _options;

    public ThresholdPruningStrategy(IOptions<VKPruningOptions> options)
    {
        _options = VKGuard.NotNull(options?.Value);
    }

    public Task<VKResult<bool>> ShouldPruneAsync(string content, double score, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(content);
        bool shouldPrune = score < _options.Threshold;
        return Task.FromResult(VKResult.Success(shouldPrune));
    }
}
