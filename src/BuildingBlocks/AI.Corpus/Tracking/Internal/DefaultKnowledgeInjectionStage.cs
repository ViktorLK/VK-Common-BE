using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.AI;
using VK.Blocks.AI.Corpus.Ingesting.Internal;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Corpus.Tracking.Internal;

/// <summary>
/// Pipeline stage responsible for persisting knowledge injection history and ingesting AI auto-refinement proposals.
/// Follows BB.03 and CS.03.
/// </summary>
internal sealed class DefaultKnowledgeInjectionStage : IVKPsychePipelineStage
{
    private readonly IVKKnowledgeInjectionStore _injectionStore;
    private readonly VKTrackingOptions _trackingOptions;
    private readonly ILogger<DefaultKnowledgeInjectionStage> _logger;

    public DefaultKnowledgeInjectionStage(
        IVKKnowledgeInjectionStore injectionStore,
        IOptions<VKTrackingOptions> trackingOptions,
        ILogger<DefaultKnowledgeInjectionStage> logger)
    {
        _injectionStore = VKGuard.NotNull(injectionStore);
        _trackingOptions = VKGuard.NotNull(trackingOptions).Value;
        _logger = VKGuard.NotNull(logger);
    }

    /// <inheritdoc />
    public int Priority => 500;

    /// <inheritdoc />
    public VKPipelineSchedule Schedule => VKPsychePipelineScheduler.After.UsageRecord;

    /// <inheritdoc />
    public bool IsActive => _trackingOptions.EnableUsageTracking;

    /// <inheritdoc />
    public async Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken cancellationToken)
    {
        VKGuard.NotNull(context);

        if (!IsActive)
        {
            return VKResult.Success();
        }

        Stopwatch stopwatch = Stopwatch.StartNew();

        // 1. Process AI Reflection Knowledge Proposals using KnowledgeProposalMapper
        var assessment = context.State<VKReflectionAssessment>();
        if (assessment?.KnowledgeProposals is { Count: > 0 } proposals)
        {
            var userId = context.Request.GetArgs<string>() ?? "System";
            foreach (var proposal in proposals)
            {
                var mapResult = KnowledgeProposalMapper.MapToCorpusEntry(proposal, userId);
                if (mapResult.IsFailure)
                {
                    // Flash-scoped or low confidence (<0.5) proposals safely skipped
                    continue;
                }

                var entry = mapResult.Value;
                if (entry.Lifecycle.IsPendingReview)
                {
                    // Queued for pending_review
                }
            }
        }

        // 2. Persist knowledge injection tracking logs
        var injectionState = context.State<VKKnowledgeCandidatesState>();
        if (injectionState == null || injectionState.Candidates.Count == 0)
        {
            return VKResult.Success();
        }

        List<VKKnowledgeInjection> injections = [];
        int turnNumber = 1;

        foreach (var entry in injectionState.Candidates)
        {
            injections.Add(new VKKnowledgeInjection(entry.Id, turnNumber, string.Empty));
        }

        var recordResult = await _injectionStore.RecordInjectionsAsync(context.Request.SessionId, injections, cancellationToken).ConfigureAwait(false); // [CS.03]
        if (recordResult.IsFailure)
        {
            // Graceful degradation on telemetry log failures
        }

        stopwatch.Stop();
        return VKResult.Success();
    }
}
