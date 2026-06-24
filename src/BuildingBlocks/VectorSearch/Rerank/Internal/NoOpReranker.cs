using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.VectorSearch.Rerank.Internal;

/// <summary>
/// Null Object implementation of IVKVectorReranker that returns candidates unchanged.
/// </summary>
internal sealed class NoOpReranker : IVKVectorReranker
{
    public Task<VKResult<IReadOnlyList<VKRerankResult>>> RerankAsync(
        VKRerankRequest request,
        CancellationToken cancellationToken = default)
    {
        // [RuleID: AP.01]
        VKGuard.NotNull(request);

        var limit = System.Math.Min(request.Candidates.Count, request.TopN);
        var results = new List<VKRerankResult>(limit);

        for (var i = 0; i < limit; i++)
        {
            var candidate = request.Candidates[i];
            results.Add(new VKRerankResult
            {
                Original = candidate,
                NewScore = candidate.Score,
                NewRank = i + 1
            });
        }

        return Task.FromResult(VKResult.Success((IReadOnlyList<VKRerankResult>)results));
    }
}
