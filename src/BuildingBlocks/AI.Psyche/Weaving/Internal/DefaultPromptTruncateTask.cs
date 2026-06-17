using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.AI.Psyche.Weaving.Diagnostics.Internal;
using VK.Blocks.Core;

// // [AP.03] Internal implementation inside Internal/ folder without VK prefix
namespace VK.Blocks.AI.Psyche.Weaving.Internal;

internal sealed class DefaultPromptTruncateTask : IVKWeavingTask
{
    private readonly IVKTokenCounter _tokenCounter;
    private readonly VKWeavingOptions _options;
    private readonly ILogger<DefaultPromptTruncateTask> _logger;

    public int TaskOrder => VKWeavingTaskOrder.Truncate;
    public bool IsParallel => false;
    public int? ParallelGroup => null;

    public DefaultPromptTruncateTask(
        IVKTokenCounter tokenCounter,
        IOptions<VKWeavingOptions> options,
        ILogger<DefaultPromptTruncateTask> logger)
    {
        _tokenCounter = VKGuard.NotNull(tokenCounter);
        _options = VKGuard.NotNull(options).Value;
        _logger = VKGuard.NotNull(logger);
    }

    public Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken ct = default)
    {
        VKGuard.NotNull(context);
        ct.ThrowIfCancellationRequested();

        var allowedBudget = context.Args<VKWeavingArgs>()?.AvailableHistoryLimit ?? _options.AvailableHistoryLimit;
        var maxTokenLimit = context.Args<VKWeavingArgs>()?.MaxTokenLimit ?? _options.MaxTokenLimit;
        var totalLimit = context.Args<VKWeavingArgs>()?.TotalContextLimit ?? _options.TotalContextLimit;

        // Ensure the total context limit does not exceed the maximum allowed token limit
        totalLimit = Math.Min(totalLimit, maxTokenLimit);

        var maxResponse = context.Args<VKWeavingArgs>()?.MaxResponseTokens ?? _options.MaxResponseTokens;

        var fragments = context.Fragments.ToList();

        // 1. Separate non-history from history fragments
        var nonHistoryFragments = fragments.Where(f => f.TierType != VKPromptTierType.Echo).ToList();
        var historyFragments = fragments.Where(f => f.TierType == VKPromptTierType.Echo).ToList();

        // 2. Count tokens of all non-history fragments first (System, Persona, Knowledge, Directives, Scenario)
        int nonHistoryTokens = 0;
        foreach (var f in nonHistoryFragments)
        {
            ct.ThrowIfCancellationRequested();
            if (f.Segment.Content is not null)
            {
                nonHistoryTokens += _tokenCounter.CountTokens(f.Segment.Content);
            }
        }

        // 3. Compute remaining history budget using strict merged rules
        int remainingHistoryBudget = totalLimit - nonHistoryTokens;
        if (remainingHistoryBudget < 0)
        {
            remainingHistoryBudget = 0;
        }

        remainingHistoryBudget = Math.Min(remainingHistoryBudget, allowedBudget);

        // 4. Sort history chronologically: highest RenderOrder (most recent) comes first
        var historySorted = historyFragments
            .OrderByDescending(f => f.RenderOrder)
            .ToList();

        var retainedHistory = new List<VKPromptFragment>();
        int activeHistoryTokens = 0;

        // 5. Retain most recent history messages up to the remaining budget
        for (int i = 0; i < historySorted.Count; i++)
        {
            ct.ThrowIfCancellationRequested();
            var hf = historySorted[i];

            // Only count if content is rendered
            int tokens = hf.Segment.Content is not null ? _tokenCounter.CountTokens(hf.Segment.Content) : 0;

            if (activeHistoryTokens + tokens <= remainingHistoryBudget)
            {
                retainedHistory.Add(hf);
                activeHistoryTokens += tokens;
            }
            else
            {
                // Track all evicted history fragments for downstream observability/events
                var evictedState = new VKPsycheEvictedState();
                context.SetState(evictedState);
                for (int j = i; j < historySorted.Count; j++)
                {
                    evictedState.Add(historySorted[j]);
                }
                break;
            }
        }

        // 6. Combine all non-history fragments and the retained chronologically-valid history fragments
        var finalFragments = new List<VKPromptFragment>(nonHistoryFragments);
        finalFragments.AddRange(retainedHistory);

        context.SetFragments(finalFragments);

        var finalEvictedState = context.State<VKPsycheEvictedState>();
        int evictedCount = finalEvictedState?.Evicted.Count ?? 0;
        if (evictedCount > 0)
        {
            WeavingDiagnostics.WeavingTruncated(_logger, context.Request.SessionId, remainingHistoryBudget, activeHistoryTokens, evictedCount);
        }

        return Task.FromResult(VKResult.Success());
    }
}
