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
internal sealed class BasicKnowledgeNarrativeManager : IVKKnowledgeManager
{
    private readonly IVKKnowledgeManager _inner;
    private readonly IVKKnowledgeNarrativeStore _narrativeStore;
    private readonly IVKKnowledgeSessionStateStore _stateStore;
    private readonly IVKKnowledgeSessionProvider _sessionProvider;

    public BasicKnowledgeNarrativeManager(
        IVKKnowledgeManager inner,
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
        string? themeId = null,
        CancellationToken cancellationToken = default)
    {
        // 1. Retrieve raw candidate entries from the core factual engine
        var coreResult = await _inner.GetRelevantEntriesAsync(context, themeId, cancellationToken).ConfigureAwait(false);
        if (coreResult.IsFailure)
        {
            return coreResult;
        }

        var candidates = coreResult.Value.ToList();
        var sessionId = _sessionProvider.GetCurrentSessionId();

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

            // Session state checks (Turn timers, Cooldowns, Sticky states)
            var stateResult = await _stateStore.GetStateAsync(sessionId, entry.Id, cancellationToken).ConfigureAwait(false);
            if (stateResult.IsSuccess && stateResult.Value != null)
            {
                var state = stateResult.Value;

                // Cooldown: check if still cooling down
                if (state.CooldownRemainingTurns > 0)
                {
                    continue; // Entry is in cooldown, prune
                }
            }

            passedEntries.Add((entry, rules));
        }

        // 3. Resolve mutual exclusion group (InclusionGroup) conflicts
        var resolvedNarrativeEntries = ResolveInclusionGroups(passedEntries);

        // 4. Combine and perform final sorting
        var finalMerged = entriesWithoutRules
            .Concat(resolvedNarrativeEntries)
            .OrderByDescending(e => e.Priority)
            .ThenByDescending(e => e.Weight)
            .Cast<VKKnowledgeEntry>();

        return VKResult.Success(finalMerged);
    }

    public Task<VKResult<IEnumerable<VKKnowledgeEntry>>> GetAllEntriesAsync(
        string? themeId = null,
        CancellationToken cancellationToken = default)
    {
        return _inner.GetAllEntriesAsync(themeId, cancellationToken);
    }

    public Task<VKResult> UpsertEntryAsync(
        VKKnowledgeEntry entry,
        string? themeId = null,
        CancellationToken cancellationToken = default)
    {
        return _inner.UpsertEntryAsync(entry, themeId, cancellationToken);
    }

    public Task<VKResult> DeleteEntryAsync(
        string entryId,
        string? themeId = null,
        CancellationToken cancellationToken = default)
    {
        return _inner.DeleteEntryAsync(entryId, themeId, cancellationToken);
    }

    #region Narrative Competitive Pruning Logic

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
                    .ThenByDescending(x => x.Entry.Priority)
                    .ThenByDescending(x => x.Entry.Weight)
                    .First();

                result.Add(winner.Entry);
            }
        }

        return result;
    }

    #endregion
}
