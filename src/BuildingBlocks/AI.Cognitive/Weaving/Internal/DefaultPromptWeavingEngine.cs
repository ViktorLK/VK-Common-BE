using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

// // [AP.03] Internal implementation inside Internal/ folder without VK prefix
namespace VK.Blocks.AI.Cognitive.Weaving.Internal;

// [AP.01] sealed default implementation
internal sealed class DefaultPromptWeavingEngine : IVKPromptWeavingEngine
{
    private readonly IVKPromptExtractionCoordinator _extractorCoordinator;
    private readonly IVKPromptScorer _scorer;
    private readonly IVKPromptPruner _pruner;
    private readonly IVKBudgetTruncator _truncator;
    private readonly IVKPromptFormatter<VKDefaultModelMarker> _formatter;
    private readonly IVKTapestryWeaver _weaver;

    public DefaultPromptWeavingEngine(
        IVKPromptExtractionCoordinator extractorCoordinator,
        IVKPromptScorer scorer,
        IVKPromptPruner pruner,
        IVKBudgetTruncator truncator,
        IVKPromptFormatter<VKDefaultModelMarker> formatter,
        IVKTapestryWeaver weaver)
    {
        _extractorCoordinator = VKGuard.NotNull(extractorCoordinator);
        _scorer = VKGuard.NotNull(scorer);
        _pruner = VKGuard.NotNull(pruner);
        _truncator = VKGuard.NotNull(truncator);
        _formatter = VKGuard.NotNull(formatter);
        _weaver = VKGuard.NotNull(weaver);
    }

    // Extract → Score&Sort → Prune → Arrange → Format → Truncate → Weave
    public async Task<VKResult<VKPromptTapestry>> WeavePromptAsync(
        VKOrchestrationPipelineContext context,
        CancellationToken cancellationToken)
    {
        VKGuard.NotNull(context);


        // 1. Extraction Phase
        var extractResult = await _extractorCoordinator.ExtractAllAsync(context, cancellationToken).ConfigureAwait(false);
        if (extractResult.IsFailure)
        {
            return VKResult.Failure<VKPromptTapestry>(extractResult.FirstError);
        }
        
        List<VKPromptFragment> fragments = new(extractResult.Value);

        context.Fragments = fragments;

        // 2. Scoring Phase
        var scoredResult = _scorer.Score(fragments, context);
        if (scoredResult.IsFailure)
            return VKResult.Failure<VKPromptTapestry>(scoredResult.FirstError);
        context.Scored = scoredResult.Value;

        // 3. Pruning Phase
        var prunedResult = _pruner.Prune(context.Scored, context);
        if (prunedResult.IsFailure)
            return VKResult.Failure<VKPromptTapestry>(prunedResult.FirstError);
        context.Pruned = prunedResult.Value;

        // 4. Truncation Phase
        var truncatedResult = _truncator.Truncate(context.Pruned, context);
        if (truncatedResult.IsFailure)
            return VKResult.Failure<VKPromptTapestry>(truncatedResult.FirstError);
        context.Truncated = truncatedResult.Value;

        // 5. Formatting Phase
        var formattedResult = _formatter.FormatContent(context.Truncated, context);
        if (formattedResult.IsFailure)
            return VKResult.Failure<VKPromptTapestry>(formattedResult.FirstError);
        context.Formatted = formattedResult.Value;

        // 6. Weaving Phase
        var tapestryResult = await _weaver.WeaveAsync(context.Formatted, context);
        if (tapestryResult.IsFailure)
            return VKResult.Failure<VKPromptTapestry>(tapestryResult.FirstError);
        context.Tapestry = tapestryResult.Value;

        return VKResult.Success(context.Tapestry);
    }
}
