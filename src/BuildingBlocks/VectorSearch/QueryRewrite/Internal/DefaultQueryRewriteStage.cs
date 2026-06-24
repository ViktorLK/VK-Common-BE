using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;
using VK.Blocks.VectorSearch.Pipeline.Internal;

namespace VK.Blocks.VectorSearch.QueryRewrite.Internal;

/// <summary>
/// Pipeline stage for rewriting queries.
/// </summary>
internal sealed class DefaultQueryRewriteStage : IVKVectorSearchBeforePipelineStage
{
    private readonly IVKQueryRewriter _strategy;
    private readonly VKQueryRewriteOptions _options;

    public DefaultQueryRewriteStage(IVKQueryRewriter strategy, IOptions<VKQueryRewriteOptions> options)
    {
        _strategy = VKGuard.NotNull(strategy);
        _options = VKGuard.NotNull(options?.Value);
    }

    public bool IsActive => _options.Enabled;

    public VKPipelineStageSchedule Schedule => VKVectorSearchPipelineScheduler.Before.QueryRewrite;

    public async Task<VKResult> ExecuteAsync(VKVectorSearchContext context, CancellationToken cancellationToken)
    {
        VKGuard.NotNull(context);

        if (context.State<SemanticCacheHitState>()?.IsHit == true)
        {
            return VKResult.Success();
        }

        var rewriteResult = await _strategy.RewriteQueryAsync(context.Query.Text, cancellationToken).ConfigureAwait(false); // [CS.03]
        if (rewriteResult.IsFailure)
        {
            return VKResult.Failure(rewriteResult.Errors);
        }

        context.Query = context.Query with { Text = rewriteResult.Value };
        return VKResult.Success();
    }
}
