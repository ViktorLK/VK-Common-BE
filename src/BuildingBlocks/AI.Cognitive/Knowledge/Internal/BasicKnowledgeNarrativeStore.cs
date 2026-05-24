using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Knowledge.Internal;

/// <summary>
/// Scoped decorator that applies advanced narrative, stochastic, and timing rules
/// on top of baseline retrieved knowledge facts.
/// </summary>
/// <remarks>
/// <b>[Architectural Design Pattern: Decorator]</b><br/>
/// Wraps the core <c>IVKKnowledgeManager</c> (i.e. <c>BasicKnowledgeManager</c>) to enrich baseline fact retrieval.<br/>
/// Registered dynamically via Scrutor <c>services.Decorate</c>, injecting the previous registration as <c>inner</c>.<br/>
/// Supports infinite chaining wrappers (e.g. Logging -> Caching -> Narrative -> Core) without class coupling.
/// </remarks>
internal sealed class BasicKnowledgeNarrativeStore : IVKKnowledgeStore
{
    private readonly IVKKnowledgeStore _inner;
    private readonly IVKKnowledgeNarrativeStore _narrativeStore;
    private readonly IVKKnowledgeSessionStateStore _stateStore;
    private readonly IVKKnowledgeSessionProvider _sessionProvider;

    public BasicKnowledgeNarrativeStore(
        IVKKnowledgeStore inner,
        IVKKnowledgeNarrativeStore narrativeStore,
        IVKKnowledgeSessionStateStore stateStore,
        IVKKnowledgeSessionProvider sessionProvider)
    {
        _inner = VKGuard.NotNull(inner);
        _narrativeStore = VKGuard.NotNull(narrativeStore);
        _stateStore = VKGuard.NotNull(stateStore);
        _sessionProvider = VKGuard.NotNull(sessionProvider);
    }

    public async Task<VKResult<IEnumerable<VKKnowledgeEntry>>> GetRelevantEntriesAsync(
        string context,
        CancellationToken cancellationToken = default)
    {
        // 1. Retrieve raw candidate entries from the core factual engine
        var coreResult = await _inner.GetRelevantEntriesAsync(context, cancellationToken).ConfigureAwait(false);
        if (coreResult.IsFailure)
        {
            return coreResult;
        }

        var candidates = coreResult.Value.ToList();
        var sessionId = _sessionProvider.GetCurrentSessionId();

        // 1.5. Check for active Sticky or Delay-Expired entries in session states
        var sessionStatesResult = await _stateStore.GetSessionStatesAsync(sessionId, cancellationToken).ConfigureAwait(false);
        if (sessionStatesResult.IsSuccess)
        {
            var stickyEntryIds = sessionStatesResult.Value
                .Where(s => s.StickyRemainingTurns > 0 || (s.DelayRemainingTurns == 0 && s.LastTriggeredTurnIndex == -1 && s.CooldownRemainingTurns == 0))
                .Select(s => s.KnowledgeId)
                .Except(candidates.Select(c => c.Id))
                .ToList();

            if (stickyEntryIds.Count > 0)
            {
                var allEntriesResult = await _inner.GetAllEntriesAsync(cancellationToken).ConfigureAwait(false);
                if (allEntriesResult.IsSuccess)
                {
                    var stickyEntries = allEntriesResult.Value.Where(e => stickyEntryIds.Contains(e.Id));
                    candidates.AddRange(stickyEntries);
                }
            }
        }

        var passedEntries = new List<(VKKnowledgeEntry Entry, VKKnowledgeNarrativeRules Rules)>();
        var entriesWithoutRules = new List<VKKnowledgeEntry>();

        // 2. Perform stochastic probability rolls and runtime timer checks
        foreach (var entry in candidates)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var rulesResult = await _narrativeStore.GetRulesAsync(entry.Id, cancellationToken).ConfigureAwait(false);
            if (rulesResult.IsFailure || rulesResult.Value == null)
            {
                // Entries without special narrative extensions pass through directly
                entriesWithoutRules.Add(entry);
                continue;
            }

            var rules = rulesResult.Value;

            // Stochastic check: Roll a random 100-sided die
            if (rules.Probability < 100)
            {
                int roll = Random.Shared.Next(1, 101);
                if (roll > rules.Probability)
                {
                    continue; // Failed the probability check, prune
                }
            }

            var stateResult = await _stateStore.GetStateAsync(sessionId, entry.Id, cancellationToken).ConfigureAwait(false);
            var state = stateResult.IsSuccess ? stateResult.Value : null;

            // Handle DelayTurns logic
            if (rules.DelayTurns > 0)
            {
                if (state == null)
                {
                    // First time keyword matched, initialize delay state
                    var newState = new VKKnowledgeSessionState
                    {
                        SessionId = sessionId,
                        KnowledgeId = entry.Id,
                        DelayRemainingTurns = rules.DelayTurns,
                        CooldownRemainingTurns = 0,
                        StickyRemainingTurns = 0,
                        LastTriggeredTurnIndex = -1
                    };
                    await _stateStore.SaveStateAsync(newState, cancellationToken).ConfigureAwait(false);
                    continue; // Skip this turn
                }
                else if (state.DelayRemainingTurns > 0)
                {
                    continue; // Still in delay, skip this turn
                }
            }

            // Session state checks (Turn timers, Cooldowns, Sticky states)
            if (state != null)
            {
                // Cooldown: check if still cooling down.
                // Note: Sticky state takes precedence over cooldown!
                if (state.CooldownRemainingTurns > 0 && state.StickyRemainingTurns <= 0)
                {
                    continue; // Entry is in cooldown and not sticky, prune
                }
            }

            passedEntries.Add((entry, rules));
        }

        // 3. Resolve mutual exclusion group (InclusionGroup) conflicts
        var resolvedNarrativeEntries = ResolveInclusionGroups(passedEntries);

        // 4. Combine and perform final sorting
        var finalMerged = entriesWithoutRules
            .Concat(resolvedNarrativeEntries)
            .OrderByDescending(e => e.Weaving.Priority)
            .ThenByDescending(e => e.Weaving.Weight)
            .Cast<VKKnowledgeEntry>();

        return VKResult.Success(finalMerged);
    }

    public Task<VKResult> UpsertEntryAsync(
        VKKnowledgeEntry entry,
        CancellationToken cancellationToken = default)
    {
        return _inner.UpsertEntryAsync(entry, cancellationToken);
    }

    public Task<VKResult> DeleteEntryAsync(
        string entryId,
        CancellationToken cancellationToken = default)
    {
        return _inner.DeleteEntryAsync(entryId, cancellationToken);
    }

    public async Task<VKResult> RecordTriggersAsync(
        string sessionId,
        IEnumerable<string> triggeredEntryIds,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotNullOrWhiteSpace(sessionId);
        VKGuard.NotNull(triggeredEntryIds);

        foreach (var entryId in triggeredEntryIds)
        {
            var rulesResult = await _narrativeStore.GetRulesAsync(entryId, cancellationToken).ConfigureAwait(false);
            if (rulesResult.IsFailure || rulesResult.Value == null)
            {
                continue;
            }

            var rules = rulesResult.Value;
            var currentStateResult = await _stateStore.GetStateAsync(sessionId, entryId, cancellationToken).ConfigureAwait(false);

            var existingState = currentStateResult.IsSuccess ? currentStateResult.Value : null;

            var newState = new VKKnowledgeSessionState
            {
                SessionId = sessionId,
                KnowledgeId = entryId,
                CooldownRemainingTurns = rules.CooldownTurns,
                StickyRemainingTurns = rules.StickyTurns,
                DelayRemainingTurns = 0, // Reset delay turns since it is now active!
                LastTriggeredTurnIndex = (existingState?.LastTriggeredTurnIndex ?? -1) + 1
            };

            await _stateStore.SaveStateAsync(newState, cancellationToken).ConfigureAwait(false);
        }

        return VKResult.Success();
    }

    public async Task<VKResult> AdvanceSessionTurnAsync(
        string sessionId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotNullOrWhiteSpace(sessionId);

        var statesResult = await _stateStore.GetSessionStatesAsync(sessionId, cancellationToken).ConfigureAwait(false);
        if (statesResult.IsFailure)
        {
            return VKResult.Failure(statesResult.Errors);
        }

        foreach (var state in statesResult.Value)
        {
            if (state.CooldownRemainingTurns > 0 || state.StickyRemainingTurns > 0 || state.DelayRemainingTurns > 0)
            {
                var newState = state with
                {
                    CooldownRemainingTurns = Math.Max(0, state.CooldownRemainingTurns - 1),
                    StickyRemainingTurns = Math.Max(0, state.StickyRemainingTurns - 1),
                    DelayRemainingTurns = Math.Max(0, state.DelayRemainingTurns - 1)
                };

                await _stateStore.SaveStateAsync(newState, cancellationToken).ConfigureAwait(false);
            }
        }

        return VKResult.Success();
    }

    private static IEnumerable<VKKnowledgeEntry> ResolveInclusionGroups(
        IEnumerable<(VKKnowledgeEntry Entry, VKKnowledgeNarrativeRules Rules)> items)
    {
        var list = items.ToList();
        var result = new List<VKKnowledgeEntry>();

        // Separate items with inclusion groups and items without
        var withGroups = list.Where(x => !string.IsNullOrWhiteSpace(x.Rules.InclusionGroup)).ToList();
        var withoutGroups = list.Where(x => string.IsNullOrWhiteSpace(x.Rules.InclusionGroup)).Select(x => x.Entry).ToList();

        result.AddRange(withoutGroups);

        if (withGroups.Count > 0)
        {
            // Group competing entries by mutual exclusion group name
            var groups = withGroups.GroupBy(x => x.Rules.InclusionGroup!, StringComparer.OrdinalIgnoreCase);

            foreach (var g in groups)
            {
                // High GroupWeight wins. If equal, higher Priority wins. If equal, higher Weight wins.
                var winner = g
                    .OrderByDescending(x => x.Rules.GroupWeight)
                    .ThenByDescending(x => x.Entry.Weaving.Priority)
                    .ThenByDescending(x => x.Entry.Weaving.Weight)
                    .First();

                result.Add(winner.Entry);
            }
        }

        return result;
    }
}
