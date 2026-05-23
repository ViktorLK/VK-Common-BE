using System.Collections.Generic;
using System.Linq;
using VK.Blocks.Core;

// // [AP.03] Internal implementation inside Internal/ folder without VK prefix
namespace VK.Blocks.AI.Cognitive.Weaving.Internal;

internal sealed class DefaultPromptWeavingEngine : IVKPromptWeavingEngine
{
    private readonly IVKPromptExtractor<IEnumerable<VKKnowledgeEntry>> _knowledgeExtractor;
    private readonly IVKPromptExtractor<IEnumerable<VKChatMessage>> _chatExtractor;
    private readonly IVKPromptScorer _scorer;
    private readonly IVKPromptPruner _pruner;
    private readonly IVKBudgetTruncator _truncator;
    private readonly IVKPromptFormatter<VKDefaultModelMarker> _formatter;
    private readonly IVKTapestryWeaver _weaver;

    public DefaultPromptWeavingEngine(
        IVKPromptExtractor<IEnumerable<VKKnowledgeEntry>> knowledgeExtractor,
        IVKPromptExtractor<IEnumerable<VKChatMessage>> chatExtractor,
        IVKPromptScorer scorer,
        IVKPromptPruner pruner,
        IVKBudgetTruncator truncator,
        IVKPromptFormatter<VKDefaultModelMarker> formatter,
        IVKTapestryWeaver weaver)
    {
        _knowledgeExtractor = VKGuard.NotNull(knowledgeExtractor);
        _chatExtractor = VKGuard.NotNull(chatExtractor);
        _scorer = VKGuard.NotNull(scorer);
        _pruner = VKGuard.NotNull(pruner);
        _truncator = VKGuard.NotNull(truncator);
        _formatter = VKGuard.NotNull(formatter);
        _weaver = VKGuard.NotNull(weaver);
    }

    public VKResult<VKPromptTapestry> WeavePrompt(VKWeavingContext context)
    {
        VKGuard.NotNull(context);

        var fragments = new List<VKPromptFragment>();

        // 1. Extraction Phase
        if (context.Pipeline.KnowledgeEntries != null && context.Pipeline.KnowledgeEntries.Any())
        {
            var knowledgeFragments = _knowledgeExtractor.Extract(context.Pipeline.KnowledgeEntries, context);
            if (knowledgeFragments.IsFailure) return VKResult.Failure<VKPromptTapestry>(knowledgeFragments.FirstError);
            fragments.AddRange(knowledgeFragments.Value);
        }

        if (context.Pipeline.Messages != null && context.Pipeline.Messages.Any())
        {
            var chatFragments = _chatExtractor.Extract(context.Pipeline.Messages, context);
            if (chatFragments.IsFailure) return VKResult.Failure<VKPromptTapestry>(chatFragments.FirstError);
            fragments.AddRange(chatFragments.Value);
        }

        context.Fragments = fragments;

        // 2. Scoring Phase
        var scoredResult = _scorer.Score(fragments, context);
        if (scoredResult.IsFailure) return VKResult.Failure<VKPromptTapestry>(scoredResult.FirstError);
        context.Scored = scoredResult.Value;

        // 3. Pruning Phase
        var prunedResult = _pruner.Prune(context.Scored, context);
        if (prunedResult.IsFailure) return VKResult.Failure<VKPromptTapestry>(prunedResult.FirstError);
        context.Pruned = prunedResult.Value;

        // 4. Truncation Phase
        var truncatedResult = _truncator.Truncate(context.Pruned, context);
        if (truncatedResult.IsFailure) return VKResult.Failure<VKPromptTapestry>(truncatedResult.FirstError);
        context.Truncated = truncatedResult.Value;

        // 5. Formatting Phase
        var formattedResult = _formatter.Format(context.Truncated, context);
        if (formattedResult.IsFailure) return VKResult.Failure<VKPromptTapestry>(formattedResult.FirstError);
        context.Formatted = formattedResult.Value;

        // 6. Weaving Phase
        var tapestryResult = _weaver.Weave(context.Formatted, context);
        if (tapestryResult.IsFailure) return VKResult.Failure<VKPromptTapestry>(tapestryResult.FirstError);
        context.Tapestry = tapestryResult.Value;

        return VKResult.Success(context.Tapestry);
    }
}
