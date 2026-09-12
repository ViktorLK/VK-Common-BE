using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VK.Blocks.AI.Psyche.Common.Internal;
using VK.Blocks.AI.Psyche.Knowledge.Diagnostics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Knowledge.Internal;

/// <summary>
/// A pipeline stage that finalizes knowledge candidates by adding them to context fragments before weaving.
/// Enforces deduplication, ordering by DepthPriority, and budget safety constraints.
/// Implements <see cref="IVKPsychePipelineStage"/>.
/// Follows BB.01 / AP.01 / AP.03 / OR.01 / BB.04.
/// </summary>
[VKTrace("psyche.stage.knowledge_finalizer")]
internal sealed class DefaultKnowledgeFinalizerStage : IVKPsychePipelineStage
{
    private readonly VKKnowledgeOptions _options;
    private readonly ILogger<DefaultKnowledgeFinalizerStage> _logger;

    public DefaultKnowledgeFinalizerStage(
        VKKnowledgeOptions options,
        ILogger<DefaultKnowledgeFinalizerStage> logger)
    {
        _options = VKGuard.NotNull(options);
        _logger = VKGuard.NotNull(logger);
    }

    /// <inheritdoc />
    public VKPipelineSchedule Schedule => VKPsychePipelineScheduler.Before.PsycheKnowledgeFinalizer;

    /// <inheritdoc />
    public bool IsActive => _options.Enabled;

    /// <inheritdoc />
    public Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken cancellationToken)
    {
        VKGuard.NotNull(context);

        var state = context.State<VKKnowledgeCandidatesState>();
        if (state is null || state.Candidates.Count == 0)
        {
            return Task.FromResult(VKResult.Success());
        }

        var maxEntries = context.Args<VKKnowledgeArgs>()?.MaxEntriesToInject ?? _options.MaxEntriesToInject;
        var reservedTokens = context.Args<VKKnowledgeArgs>()?.ReservedTokens ?? _options.ReservedTokens;

        var orderedCandidates = state.Candidates
            .DistinctBy(e => e.Id)
            .OrderBy(e => e.Segment.DepthPriority)
            .ToList();

        int candidateCount = orderedCandidates.Count;
        int injectedCount = 0;
        int accumulatedTokens = 0;

        foreach (var entry in orderedCandidates)
        {
            if (maxEntries is > 0 && injectedCount >= maxEntries.Value)
            {
                break;
            }

            if (reservedTokens is > 0 && entry.Segment.TokenCount > 0)
            {
                if (accumulatedTokens + entry.Segment.TokenCount > reservedTokens.Value && injectedCount > 0)
                {
                    break;
                }
            }

            if (string.IsNullOrWhiteSpace(entry.Segment.Content))
            {
                continue;
            }

            var segment = entry.Segment with { Tier = VKPromptTierType.Knowledge };
            if (string.IsNullOrWhiteSpace(segment.TagName))
            {
                segment = segment with { TagName = PsycheConstants.XmlTags.Knowledge };
            }

            context.AddSegment(segment);
            injectedCount++;
            accumulatedTokens += entry.Segment.TokenCount;
        }

        var truncatedCount = Math.Max(0, candidateCount - injectedCount);
        _logger.KnowledgeFinalized(context.Request.SessionId, context.CorrelationId, injectedCount, truncatedCount, accumulatedTokens);

        Activity.Current?.SetPsycheKnowledgeFinalizedCount(injectedCount, truncatedCount);

        return Task.FromResult(VKResult.Success());
    }
}
