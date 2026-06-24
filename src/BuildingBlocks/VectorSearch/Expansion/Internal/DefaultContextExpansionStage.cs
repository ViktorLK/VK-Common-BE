using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;
using VK.Blocks.VectorSearch.Pipeline.Internal;

namespace VK.Blocks.VectorSearch.Expansion.Internal;

/// <summary>
/// Pipeline stage for expanding the context of search results.
/// </summary>
internal sealed class DefaultContextExpansionStage : IVKVectorSearchAfterPipelineStage
{
    private readonly IVKContextExpansionStrategy _strategy;
    private readonly VKContextExpansionOptions _options;

    public DefaultContextExpansionStage(IVKContextExpansionStrategy strategy, IOptions<VKContextExpansionOptions> options)
    {
        _strategy = VKGuard.NotNull(strategy);
        _options = VKGuard.NotNull(options?.Value);
    }

    public bool IsActive => _options.Enabled;

    public VKPipelineStageSchedule Schedule => VKVectorSearchPipelineScheduler.After.ContextExpansion;

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

        var expansionResult = await _strategy.ExpandContextAsync(context.Results, cancellationToken).ConfigureAwait(false); // [CS.03]
        if (expansionResult.IsFailure)
        {
            return VKResult.Failure(expansionResult.Errors);
        }

        context.Results = expansionResult.Value;
        return VKResult.Success();
    }
}
