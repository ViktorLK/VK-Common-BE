using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VK.Blocks.AI.Psyche.Weaving.Diagnostics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Weaving.Internal;

internal sealed class DefaultPromptTruncateTask : IVKWeavingPipelineTask
{
    private readonly IVKTokenCounter _tokenCounter;
    private readonly IVKModelCatalog _modelCatalog;
    private readonly VKWeavingOptions _options;
    private readonly ILogger<DefaultPromptTruncateTask> _logger;

    public VKPipelineSchedule Schedule => new(VKWeavingTaskOrder.Truncate);

    public DefaultPromptTruncateTask(
        IVKTokenCounter tokenCounter,
        IVKModelCatalog modelCatalog,
        VKWeavingOptions options,
        ILogger<DefaultPromptTruncateTask> logger)
    {
        _tokenCounter = VKGuard.NotNull(tokenCounter);
        _modelCatalog = VKGuard.NotNull(modelCatalog);
        _options = VKGuard.NotNull(options);
        _logger = VKGuard.NotNull(logger);
    }

    public Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(context);

        // 1. Dynamically resolve physical model metadata via IVKModelCatalog
        var modelId = context.Args<VKChatArgs>()?.ModelId ?? string.Empty;
        var modelMetadata = _modelCatalog.GetModelMetadata(modelId);

        // Total Limit = MaxContextBudget (from args or options) ?? Physical Model Context Window
        var configuredBudget = context.Args<VKWeavingArgs>()?.MaxContextBudget ?? _options.MaxContextBudget;
        var totalLimit = configuredBudget.HasValue
            ? Math.Min(configuredBudget.Value, modelMetadata.ContextWindowSize)
            : modelMetadata.ContextWindowSize;

        var fragments = context.Fragments.ToList();

        // 2. Separate non-history from history fragments
        var nonHistoryFragments = fragments.Where(f => f.TierType != VKPromptTierType.Echo).ToList();
        var historyFragments = fragments.Where(f => f.TierType == VKPromptTierType.Echo).ToList();

        // 3. Count tokens of all non-history fragments first (System, Persona, Knowledge, Directives, Scenario)
        int nonHistoryTokens = 0;
        foreach (var f in nonHistoryFragments)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (f.Segment.Content is not null)
            {
                nonHistoryTokens += _tokenCounter.CountTokens(f.Segment.Content);
            }
        }

        // 4. Compute remaining history budget using strict merged rules & model metadata
        int remainingHistoryBudget = totalLimit - nonHistoryTokens;
        if (remainingHistoryBudget < 0)
        {
            remainingHistoryBudget = 0;
        }

        // 5. Sort history chronologically: highest RenderOrder (most recent) comes first
        var historySorted = historyFragments
            .OrderByDescending(f => f.RenderOrder)
            .ToList();

        var retainedHistory = new List<VKPromptFragment>();
        int activeHistoryTokens = 0;

        // 6. Retain most recent history messages up to the remaining budget
        for (int i = 0; i < historySorted.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
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

        // 7. Combine all non-history fragments and the retained chronologically-valid history fragments
        var finalFragments = new List<VKPromptFragment>(nonHistoryFragments);
        finalFragments.AddRange(retainedHistory);

        context.SetFragments(finalFragments);

        var finalEvictedState = context.State<VKPsycheEvictedState>();
        int evictedCount = finalEvictedState?.Evicted.Count ?? 0;
        if (evictedCount > 0)
        {
            _logger.WeavingTruncated(context.Request.SessionId, remainingHistoryBudget, activeHistoryTokens, evictedCount);
        }

        return Task.FromResult(VKResult.Success());
    }
}
