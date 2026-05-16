using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Vectorics.ReRanking.Internal;

/// <summary>
/// No-op implementation of <see cref="IVKReRanker"/>.
/// Returns candidates in their original order with neutral scores.
/// </summary>
internal sealed class NoOpVKReRanker : IVKReRanker
{
    // [SG Hook]
    public Task<VKResult<IReadOnlyList<VKReRankingResult>>> ReRankAsync(
        string query,
        IEnumerable<string> candidates,
        VKReRankingArgs? args = null,
        CancellationToken cancellationToken = default)
    {
        _ = query;
        _ = args;
        _ = cancellationToken;

        var result = candidates
            .Select((c, i) => new VKReRankingResult(c, 1.0f - (i * 0.001f), i))
            .ToList();

        return Task.FromResult(VKResult.Success<IReadOnlyList<VKReRankingResult>>(result));
    }
}
