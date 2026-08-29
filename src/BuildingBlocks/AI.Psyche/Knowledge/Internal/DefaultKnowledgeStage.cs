using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using VK.Blocks.AI.Psyche.Knowledge.Diagnostics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Knowledge.Internal;

[VKTrace("psyche.stage.knowledge")]
internal sealed class DefaultKnowledgeStage : IVKPsychePipelineStage
{
    private readonly VKKnowledgeOptions _options;
    private readonly IVKPsycheKnowledgeRepository _knowledgeRepository;
    private readonly IVKKnowledgeRenderer _renderer;
    private readonly VKWeavingOptions _weavingOptions;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<DefaultKnowledgeStage> _logger;

    public VKPipelineSchedule Schedule => VKPsychePipelineScheduler.Before.PsycheKnowledge;
    public bool IsActive => _options.Enabled;

    public DefaultKnowledgeStage(
        VKKnowledgeOptions options,
        IVKPsycheKnowledgeRepository knowledgeRepository,
        IVKKnowledgeRenderer renderer,
        VKWeavingOptions weavingOptions,
        TimeProvider? timeProvider = null,
        ILogger<DefaultKnowledgeStage>? logger = null)
    {
        _options = VKGuard.NotNull(options);
        _knowledgeRepository = VKGuard.NotNull(knowledgeRepository);
        _renderer = VKGuard.NotNull(renderer);
        _weavingOptions = VKGuard.NotNull(weavingOptions);
        _timeProvider = timeProvider ?? TimeProvider.System;
        _logger = logger ?? NullLogger<DefaultKnowledgeStage>.Instance;
    }

    public async Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken ct)
    {
        VKGuard.NotNull(context);

        var disabledTiers = context.Args<VKWeavingArgs>()?.DisabledTiers ?? _weavingOptions.DisabledTiers;
        if (disabledTiers is not null && disabledTiers.Contains(VKPromptTierType.Knowledge))
        {
            return VKResult.Success();
        }

        if (context.Request.KnowledgeIds.Count == 0)
        {
            return VKResult.Success();
        }

        var knowledgeResult = await _knowledgeRepository.ListByIdsAsync(context.Request.KnowledgeIds, ct).ConfigureAwait(false); // [CS.03]
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

        // Session-level state tracking
        var sessionThread = context.State<VKSessionThread>();
        var currentTurn = sessionThread?.TurnCount ?? 0;
        var existingKnowledgeState = sessionThread?.KnowledgeState;

        var updatedTriggeredTurns = existingKnowledgeState?.LastTriggeredTurns is not null
            ? new Dictionary<VKKnowledgeId, int>(existingKnowledgeState.LastTriggeredTurns)
            : new Dictionary<VKKnowledgeId, int>();

        // 1. Maintain entries that are still within turn retention window
        if (_options.KeywordScanDepth > 0 && existingKnowledgeState?.LastTriggeredTurns is not null)
        {
            foreach (var (knowledgeId, triggeredTurn) in existingKnowledgeState.LastTriggeredTurns)
            {
                if (currentTurn - triggeredTurn < _options.KeywordScanDepth)
                {
                    var retainedEntry = candidateEntries.FirstOrDefault(e => e.Id == knowledgeId);
                    if (retainedEntry is not null && !activeEntries.Contains(retainedEntry))
                    {
                        activeEntries.Add(retainedEntry);
                    }
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
            sessionThread.AdvanceKnowledgeState(updatedKnowledgeState, _timeProvider.GetUtcNow());
        }
        context.SetState(updatedKnowledgeState);

        var candidateState = context.State<VKKnowledgeCandidatesState>();
        if (candidateState is null)
        {
            candidateState = new VKKnowledgeCandidatesState();
            context.SetState(candidateState);
        }
        candidateState.Candidates.AddRange(activeEntries);

        Activity.Current?.SetPsycheKnowledgeCount(activeEntries.Count);
        if (activeEntries.Count > 0)
        {
            KnowledgeDiagnostics.RecordEntriesMatched(activeEntries.Count, "Knowledge", "Keyword+Constant");
        }
        _logger.KnowledgeMatched(activeEntries.Count, context.Request.SessionId.Value.ToString(), context.CorrelationId, 0);

        return VKResult.Success();
    }
}
