using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.AI.Corpus.Common.Models.Internal;
using VK.Blocks.AI.Corpus.Diagnostics.Internal;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Corpus.Tracking.Internal;

/// <summary>
/// Pipeline stage that runs AFTER the LLM call to record injected knowledge usage.
/// Implements <see cref="IVKPsycheAfterPipelineStage"/>.
/// Follows BB.01 / AP.03.
/// </summary>
internal sealed class DefaultKnowledgeInjectionStage : IVKPsycheAfterPipelineStage
{
    private readonly IVKKnowledgeInjectionStore _usageStore;
    private readonly VKTrackingOptions _trackingOptions;
    private readonly VKAICorpusOptions _corpusOptions;
    private readonly ILogger<DefaultKnowledgeInjectionStage> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="DefaultKnowledgeInjectionStage"/>.
    /// </summary>
    public DefaultKnowledgeInjectionStage(
        IVKKnowledgeInjectionStore usageStore,
        IOptions<VKTrackingOptions> trackingOptions,
        IOptions<VKAICorpusOptions> corpusOptions,
        ILogger<DefaultKnowledgeInjectionStage> logger)
    {
        _usageStore = VKGuard.NotNull(usageStore);
        _trackingOptions = VKGuard.NotNull(trackingOptions?.Value);
        _corpusOptions = VKGuard.NotNull(corpusOptions?.Value);
        _logger = VKGuard.NotNull(logger);
    }

    /// <inheritdoc />
    public VKStageSchedule Schedule => VKPsychePipelineScheduler.After.UsageRecord;

    /// <inheritdoc />
    public bool IsActive => _corpusOptions.Enabled && _trackingOptions.EnableUsageTracking;

    /// <inheritdoc />
    public async Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken cancellationToken)
    {
        VKGuard.NotNull(context);

        if (!IsActive)
        {
            return VKResult.Success();
        }

        CorpusInjectionState? state = context.State<CorpusInjectionState>();
        if (state == null || state.InjectedEntries.Count == 0)
        {
            return VKResult.Success();
        }

        List<VKKnowledgeInjection> injections = [];
        foreach (VKKnowledgeLifecycleEntry entry in state.InjectedEntries)
        {
            injections.Add(new VKKnowledgeInjection(
                entry.Knowledge.Id,
                state.CurrentTurn,
                entry.Lifecycle.GroupId ?? string.Empty));
        }

        VKResult recordResult = await _usageStore.RecordInjectionsAsync(
            context.Request.SessionId,
            injections,
            cancellationToken).ConfigureAwait(false); // [CS.03]

        if (!recordResult.IsSuccess)
        {
            CorpusLog.FailedToRecordInjections(
                _logger,
                context.Request.SessionId.Value.ToString(),
                recordResult.FirstError.Description);
        }

        return VKResult.Success();
    }
}
