using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;
using VK.Blocks.VectorSearch.Pipeline.Internal;

namespace VK.Blocks.VectorSearch.Rerank.Internal;

/// <summary>
/// Pipeline stage for reranking search results using standard core contracts.
/// </summary>
internal sealed class DefaultRerankStage : IVKVectorSearchAfterPipelineStage
{
    private readonly IVKVectorReranker _reRanker;
    private readonly VKVectorRerankingOptions _options;

    public DefaultRerankStage(IVKVectorReranker reRanker, IOptions<VKVectorRerankingOptions> options)
    {
        _reRanker = VKGuard.NotNull(reRanker);
        _options = VKGuard.NotNull(options?.Value);
    }

    public bool IsActive => _options.Enabled;

    public VKPipelineStageSchedule Schedule => VKVectorSearchPipelineScheduler.After.Rerank;

    public async Task<VKResult> ExecuteAsync(VKVectorSearchContext context, CancellationToken cancellationToken)
    {
        VKGuard.NotNull(context);

        if (context.State<SemanticCacheHitState>()?.IsHit == true)
        {
            return VKResult.Success();
        }

        if (context.Results.Length == 0)
        {
            return VKResult.Success();
        }

        var request = new VKRerankRequest
        {
            Query = context.Query.Text,
            Candidates = context.Results,
            TopN = context.Results.Length // In stage pipeline, we rerank all candidates by default
        };

        var rerankResult = await _reRanker.RerankAsync(request, cancellationToken).ConfigureAwait(false); // [CS.03]
        if (rerankResult.IsFailure)
        {
            return VKResult.Failure(rerankResult.Errors);
        }

        var reranked = rerankResult.Value.Select(r => new VKSearchResult
        {
            Document = r.Original.Document,
            Score = (float)r.NewScore
        }).ToArray(); // [AP.01]

        context.Results = reranked;
        return VKResult.Success();
    }
}
