using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram.Scoring.Internal;

/// <summary>
/// Pipeline stage for scoring engrams.
/// </summary>
internal sealed class DefaultScoringStage
{
    private readonly IVKScoringStrategy _strategy;
    private readonly VKScoringOptions _options;

    public DefaultScoringStage(IVKScoringStrategy strategy, IOptions<VKScoringOptions> options)
    {
        _strategy = VKGuard.NotNull(strategy);
        _options = VKGuard.NotNull(options?.Value);
    }

    public async Task<VKResult<double>> ProcessAsync(string content, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(content);

        if (!_options.Enabled)
        {
            return VKResult.Success(0.0);
        }

        return await _strategy.ScoreAsync(content, cancellationToken).ConfigureAwait(false); // [CS.03]
    }
}
