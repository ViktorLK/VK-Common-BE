using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using VK.Blocks.AI.Psyche.Common.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Knowledge.Internal;

internal sealed class DefaultKnowledgeStage : IVKPsycheBeforePipelineStage
{
    private readonly VKKnowledgeOptions _options;
    private readonly IVKKnowledgeStore _store;
    private readonly VKWeavingOptions _weavingOptions;

    public int StageOrder => VKWeavingStageOrder.Knowledge;

    public bool IsActive => _options.Enabled;

    public bool IsParallel => true;

    public int? ParallelGroup => 2;

    public DefaultKnowledgeStage(
        IOptions<VKKnowledgeOptions> options,
        IVKKnowledgeStore store,
        IOptions<VKWeavingOptions> weavingOptions)
    {
        _options = VKGuard.NotNull(options).Value;
        _store = VKGuard.NotNull(store);
        _weavingOptions = VKGuard.NotNull(weavingOptions?.Value);
    }

    public async Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken ct)
    {
        VKGuard.NotNull(context);

        var disabledTiers = context.Args<VKWeavingArgs>()?.DisabledTiers ?? _weavingOptions.DisabledTiers;
        if (disabledTiers is not null && disabledTiers.Contains(VKPromptTierType.Knowledge))
        {
            return VKResult.Success();
        }

        if (context.Request.PersonaId.IsEmpty)
        {
            return VKResult.Failure(VKKnowledgeErrors.MissingPersona);
        }

        var knowledgeResult = await _store.GetRelevantEntriesAsync(context.Request.PersonaId, ct).ConfigureAwait(false); // [CS.03]
        if (knowledgeResult.IsFailure)
        {
            return VKResult.Failure(knowledgeResult.Errors); // [CS.01]
        }

        var candidateEntries = knowledgeResult.Value.Where(e => e.IsEnabled).ToList();

        // Separate constant entries from conditional keyword/regex entries
        // Constant entries are permanently active and do not need to run through the chronological matching simulation.
        var activeEntries = candidateEntries
            .Where(e => e.TriggerType == VKKnowledgeTriggerType.Constant)
            .ToList();

        var conditionalEntries = candidateEntries
            .Where(e => e.TriggerType != VKKnowledgeTriggerType.Constant)
            .ToList();

        // 1. Fetch dialog history from context fragments to simulate the chronological timeline
        var echoes = context.Fragments
            .Where(f => f.TierType == VKPromptTierType.Echo && f.Metadata is VKEchoTrace)
            .OrderBy(f => f.RenderOrder)
            .Select(f => (VKEchoTrace)f.Metadata!)
            .ToList();

        // 2. Build the simulation timeline with mapped user-based turn indices
        var timeline = new List<(string Content, int TurnIndex)>();
        int currentTurn = 0;
        bool hasUserInCurrentTurn = false;

        foreach (var echo in echoes)
        {
            if (echo.Role == VKChatRole.User)
            {
                if (hasUserInCurrentTurn)
                {
                    currentTurn++;
                }
                hasUserInCurrentTurn = true;
            }
            timeline.Add((echo.Content, currentTurn));
        }

        if (!string.IsNullOrWhiteSpace(context.Request.UserInput))
        {
            if (hasUserInCurrentTurn)
            {
                currentTurn++;
            }
            timeline.Add((context.Request.UserInput, currentTurn));
        }

        var lastTriggeredTurn = new Dictionary<VKKnowledgeId, int>(); // Key: KnowledgeId, Value: TurnIndex

        /*
         * PERFORMANCE ARCHITECTURAL NOTE:
         * Currently, we perform a full scan of the dialogue history (O(Timeline * Entries)) to calculate CooldownTurns and StickyTurns.
         * For typical chat limits (under 30 echoes) and small entry sets, this is extremely fast (<2ms) due to high-performance expression compilations.
         *
         * OPTIMIZATION STRATEGY:
         * If the system ever scales to support massive knowledgebases (1000s of entries) and long dialogue histories (100s of echoes),
         * under heavy parallel loads with strict latency requirements, we should transition to "Incremental State Tracking":
         * 1. Persist the `lastTriggeredTurn` state dictionary and `currentTurnIndex` inside the session state/database.
         * 2. On each new turn, perform an incremental O(1 * Entries) match ONLY on the latest incoming UserInput / last Assistant Echo.
         * 3. Update and persist the state, and resolve active entries directly from the state offset.
         */

        // 3. Simulate timeline keyword triggers matching chronologically (only for conditional entries)
        for (int i = 0; i < timeline.Count; i++)
        {
            var (turnContent, turnIndex) = timeline[i];

            foreach (var entry in conditionalEntries)
            {
                // Check if the entry is currently in cooldown (or has infinite cooldown -1)
                if (lastTriggeredTurn.TryGetValue(entry.Id, out var lastTurn) &&
                    (entry.CooldownTurns == -1 || turnIndex - lastTurn < entry.CooldownTurns))
                {
                    continue; // Skip triggering during cooldown
                }

                var matcher = VKKnowledgeMatcher.GetMatcher(entry);
                if (matcher(turnContent))
                {
                    lastTriggeredTurn[entry.Id] = turnIndex;
                }
            }
        }

        int currentTurnIndex = currentTurn; // Index of the latest active turn

        // 4. Filter conditional knowledge entries that are active in the current turn and merge with constant ones
        var activeConditionals = conditionalEntries.Where(entry =>
        {
            if (lastTriggeredTurn.TryGetValue(entry.Id, out var lastTurn))
            {
                // Verify if it was triggered after the DelayTurns offset and is within the StickyTurns window (-1 means infinite)
                int elapsed = currentTurnIndex - lastTurn;
                return elapsed >= entry.DelayTurns &&
                       (entry.StickyTurns == -1 || elapsed <= (entry.DelayTurns + entry.StickyTurns));
            }
            return false;
        });

        activeEntries.AddRange(activeConditionals);

        // 4b. Perform exclusive group pruning: if entries belong to the same group, retain only the one with the highest weight
        var prunedActive = new List<VKKnowledgeEntry>();
        var exclusiveGroups = activeEntries.GroupBy(e => e.ExclusiveGrouping?.Group);
        foreach (var group in exclusiveGroups)
        {
            if (group.Key is null)
            {
                prunedActive.AddRange(group);
            }
            else
            {
                var survivor = group.OrderByDescending(e => e.ExclusiveGrouping!.Weight).FirstOrDefault();
                if (survivor is not null)
                {
                    prunedActive.Add(survivor);
                }
            }
        }
        activeEntries = prunedActive;

        var groups = activeEntries
            .GroupBy(entry =>
            {
                var coord = PromptPositionResolver.Resolve(entry.Position, entry.Priority, PromptLayout.DefaultRenderOrders);
                return (Role: coord.Role, Depth: coord.Depth, RenderOrderOffset: coord.RenderOrder);
            });

        foreach (var group in groups)
        {
            var key = group.Key;
            var entriesForSlot = group.ToList();

            for (int i = 0; i < entriesForSlot.Count; i++)
            {
                context.AddFragment(new VKPromptFragment
                {
                    TierType = VKPromptTierType.Knowledge,
                    Role = key.Role,
                    Depth = key.Depth,
                    RenderOrder = key.RenderOrderOffset + i,
                    Metadata = entriesForSlot[i]
                });
            }
        }

        return VKResult.Success();
    }
}
