using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Knowledge.Internal;

internal sealed class DefaultKnowledgeStage : IVKPsychePipelineStage
{
    private readonly VKKnowledgeOptions _options;
    private readonly IVKKnowledgeStore _store;
    private readonly VKWeavingOptions _weavingOptions;

    public VKPipelineSchedule Schedule => VKPsychePipelineScheduler.Before.PsycheKnowledge;
    public bool IsActive => _options.Enabled;

    public DefaultKnowledgeStage(
        VKKnowledgeOptions options,
        IVKKnowledgeStore store,
        VKWeavingOptions weavingOptions)
    {
        _options = VKGuard.NotNull(options);
        _store = VKGuard.NotNull(store);
        _weavingOptions = VKGuard.NotNull(weavingOptions);
    }

    public async Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken ct)
    {
        VKGuard.NotNull(context);

        var stopwatch = Stopwatch.StartNew();
        try
        {
            var disabledTiers = context.Args<VKWeavingArgs>()?.DisabledTiers ?? _weavingOptions.DisabledTiers;
            if (disabledTiers is not null && disabledTiers.Contains(VKPromptTierType.Knowledge))
            {
                return VKResult.Success();
            }

            if (context.Request.KnowledgeIds.Count == 0)
            {
                return VKResult.Success();
            }

            var knowledgeResult = await _store.GetKnowledgeEntriesAsync(context.Request.KnowledgeIds, ct).ConfigureAwait(false); // [CS.03]
            if (knowledgeResult.IsFailure)
            {
                return VKResult.Failure(knowledgeResult.Errors); // [CS.01]
            }

            var candidateEntries = knowledgeResult.Value.Where(e => e.Segment.IsEnabled).ToList();

            // Separate constant entries from conditional keyword/regex entries
            var activeEntries = candidateEntries
                .Where(e => e.TriggerType == VKKnowledgeTriggerType.Constant)
                .ToList();

            var conditionalEntries = candidateEntries
                .Where(e => e.TriggerType != VKKnowledgeTriggerType.Constant)
                .ToList();

            // Retrieve current Session state and TurnCount
            var sessionThread = context.State<VKSessionThread>();
            var currentTurn = sessionThread?.TurnCount ?? 0;

            var sessionKnowledgeState = sessionThread?.KnowledgeState ?? context.State<VKSessionKnowledgeState>() ?? new VKSessionKnowledgeState();
            var updatedTriggeredTurns = new Dictionary<VKKnowledgeId, int>(sessionKnowledgeState.LastTriggeredTurns);

            var keywordScanDepth = _options.KeywordScanDepth;

            // 1. Process active entries from previous turns within sliding window
            if (keywordScanDepth != 0 && sessionKnowledgeState.LastTriggeredTurns.Count > 0)
            {
                var entriesById = conditionalEntries.ToDictionary(e => e.Id);

                foreach (var (knowledgeId, lastTurn) in sessionKnowledgeState.LastTriggeredTurns)
                {
                    var elapsedTurns = currentTurn - lastTurn;
                    var maxAllowedWindow = keywordScanDepth == -1 ? int.MaxValue : keywordScanDepth;

                    if (elapsedTurns <= maxAllowedWindow && entriesById.TryGetValue(knowledgeId, out var entry))
                    {
                        if (!activeEntries.Contains(entry))
                        {
                            activeEntries.Add(entry);
                        }
                    }
                    else
                    {
                        // Prune out-of-window entries from tracking state
                        updatedTriggeredTurns.Remove(knowledgeId);
                    }
                }
            }

            // 2. Incremental scan on current UserInput
            if (!string.IsNullOrWhiteSpace(context.Request.UserInput) && conditionalEntries.Count > 0)
            {
                var userInput = context.Request.UserInput;
                foreach (var entry in conditionalEntries)
                {
                    var matcher = DefaultKnowledgeMatcher.GetMatcher(entry);
                    if (matcher(userInput))
                    {
                        if (!activeEntries.Contains(entry))
                        {
                            activeEntries.Add(entry);
                        }
                        updatedTriggeredTurns[entry.Id] = currentTurn;
                    }
                }
            }

            // Update context states for downstream consumption & session persistence
            var updatedKnowledgeState = new VKSessionKnowledgeState
            {
                LastEvaluatedTurn = currentTurn,
                LastTriggeredTurns = updatedTriggeredTurns
            };

            if (sessionThread is not null)
            {
                context.SetState(sessionThread with { KnowledgeState = updatedKnowledgeState });
            }
            context.SetState(updatedKnowledgeState);

            var candidateState = context.State<VKKnowledgeCandidatesState>();
            if (candidateState is null)
            {
                candidateState = new VKKnowledgeCandidatesState();
                context.SetState(candidateState);
            }
            candidateState.Candidates.AddRange(activeEntries);

            return VKResult.Success();
        }
        finally
        {
            stopwatch.Stop();
            context.ResponseBuilder.ProfilingMetrics["KnowledgeStage"] = stopwatch.Elapsed.TotalMilliseconds;
        }
    }
}
