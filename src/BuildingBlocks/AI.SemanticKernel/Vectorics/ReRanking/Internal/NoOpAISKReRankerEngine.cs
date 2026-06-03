using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.SemanticKernel.Vectorics.ReRanking.Internal;

/// <summary>
/// A no-op implementation of <see cref="IVKReRanker"/> used when the feature is disabled.
/// Returns the original candidates unmodified, scoring them sequentially downwards.
/// </summary>
internal sealed class NoOpAISKReRankerEngine : IVKReRanker
{
    /// <inheritdoc />
    public Task<VKResult<IReadOnlyList<VKReRankingResult>>> ReRankAsync(
        string query,
        IEnumerable<string> candidates,
        VKReRankingArgs? args = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(query);
        VKGuard.NotNull(candidates);

        var candidateList = candidates.ToList();
        var results = new List<VKReRankingResult>(candidateList.Count);

        for (int i = 0; i < candidateList.Count; i++)
        {
            // Give an arbitrary descending score when No-Op
            float dummyScore = 1.0f - ((float)i / candidateList.Count);
            results.Add(new VKReRankingResult(candidateList[i], dummyScore, i));
        }

        return Task.FromResult(VKResult.Success<IReadOnlyList<VKReRankingResult>>(results));
    }
}
