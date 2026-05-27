using System;
using System.Collections.Generic;
using System.Linq;
using VK.Blocks.Core;

// // [AP.03] Internal implementation inside Internal/ folder without VK prefix
namespace VK.Blocks.AI.Cognitive.Weaving.Internal;

internal sealed class BasicBudgetTruncator : IVKBudgetTruncator
{
    private readonly IVKMemoryEvictionDispatcher? _evictionDispatcher;
    private readonly TimeProvider? _timeProvider;

    public BasicBudgetTruncator(IVKMemoryEvictionDispatcher? evictionDispatcher = null, TimeProvider? timeProvider = null)
    {
        _evictionDispatcher = evictionDispatcher;
        _timeProvider = timeProvider;
    }

    public VKResult<IReadOnlyList<VKScoredFragment>> Truncate(IReadOnlyList<VKScoredFragment> pruned, VKOrchestrationPipelineContext context)
    {
        VKGuard.NotNull(pruned);
        VKGuard.NotNull(context);

        var budgetPlan = context.TokenBudget;
        if (budgetPlan == null)
        {
            return VKResult.Success<IReadOnlyList<VKScoredFragment>>(pruned);
        }

        var tokenMeter = budgetPlan.TokenMeterResolver?.Invoke();

        // If no token meter available or no history limit, pass-through
        if (tokenMeter == null || budgetPlan.AvailableHistoryLimit <= 0)
        {
            return VKResult.Success<IReadOnlyList<VKScoredFragment>>(pruned);
        }

        var allowedBudget = budgetPlan.AvailableHistoryLimit;

        // Split fragments into non-history and history. For simplicity, assume all fragments have tokens counted.
        // In reality, this requires distinguishing history vs knowledge.
        int nonHistoryTokens = 0;
        var nonHistoryFragments = pruned.Where(p => p.Fragment.TierType != VKPromptTierType.ChatHistory).ToList();

        foreach (var f in nonHistoryFragments)
        {
            nonHistoryTokens += tokenMeter.CountTokens(f.Fragment.Content);
        }

        int remainingHistoryBudget = allowedBudget - nonHistoryTokens;
        if (remainingHistoryBudget < 0)
            remainingHistoryBudget = 0;

        var historyFragments = pruned.Where(p => p.Fragment.TierType == VKPromptTierType.ChatHistory)
                                     .OrderBy(p => p.Fragment.Depth) // Order by depth to keep most recent
                                     .ToList();

        var retainedFragments = new List<VKScoredFragment>(nonHistoryFragments);
        int activeHistoryTokens = 0;

        foreach (var hf in historyFragments)
        {
            int tokens = tokenMeter.CountTokens(hf.Fragment.Content);
            if (activeHistoryTokens + tokens <= remainingHistoryBudget)
            {
                retainedFragments.Add(hf);
                activeHistoryTokens += tokens;
            }
            else
            {
                // Truncated logic met: evict the rest
                if (_evictionDispatcher != null && _timeProvider != null)
                {
                    // Everything from current index in historyFragments to the end is evicted.
                    // But remember we ordered by depth (most recent first). So the ones that don't fit are the oldest!
                    var evictedMsg = new VKChatMessage { Role = VKChatRole.User, Content = hf.Fragment.Content };

                    var evictionEvent = new VKMemoryEvictionEvent
                    {
                        SessionId = context.SessionId,
                        TenantId = context.GovernanceSnapshot?.TenantId ?? "Default",
                        UserId = context.GovernanceSnapshot?.UserId ?? "Anonymous",
                        EvictedMessages = new List<VKChatMessage> { evictedMsg }, // Can aggregate all evicted here
                        OccurredAt = _timeProvider.GetUtcNow()
                    };
                    _ = _evictionDispatcher.DispatchAsync(evictionEvent, System.Threading.CancellationToken.None).AsTask();
                }
            }
        }

        return VKResult.Success<IReadOnlyList<VKScoredFragment>>(retainedFragments);
    }
}
