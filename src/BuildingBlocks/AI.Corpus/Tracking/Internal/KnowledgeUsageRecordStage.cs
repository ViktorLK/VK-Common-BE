using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.AI.Corpus.Diagnostics.Internal;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Corpus.Tracking.Internal;

/// <summary>
/// Pipeline stage that runs AFTER the LLM call to record injected knowledge usage.
/// Implements <see cref="IVKPsycheAfterPipelineStage"/>.
/// Follows BB.01 / AP.03.
/// </summary>
internal sealed class KnowledgeUsageRecordStage : IVKPsycheAfterPipelineStage
{
    private readonly IVKKnowledgeUsageStore _usageStore;
    private readonly VKCorpusTrackingOptions _trackingOptions;
    private readonly VKCorpusOptions _corpusOptions;
    private readonly ILogger<KnowledgeUsageRecordStage> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="KnowledgeUsageRecordStage"/>.
    /// </summary>
    public KnowledgeUsageRecordStage(
        IVKKnowledgeUsageStore usageStore,
        IOptions<VKCorpusTrackingOptions> trackingOptions,
        IOptions<VKCorpusOptions> corpusOptions,
        ILogger<KnowledgeUsageRecordStage> logger)
    {
        _usageStore = VKGuard.NotNull(usageStore);
        _trackingOptions = VKGuard.NotNull(trackingOptions?.Value);
        _corpusOptions = VKGuard.NotNull(corpusOptions?.Value);
        _logger = VKGuard.NotNull(logger);
    }

    /// <inheritdoc />
    public int StageOrder => VKPsychePipelineScheduler.After.UsageRecord.Order;

    /// <inheritdoc />
    public bool IsActive => _corpusOptions.Enabled && _trackingOptions.EnableUsageTracking;

    /// <inheritdoc />
    public bool IsParallel => false;

    /// <inheritdoc />
    public int? ParallelGroup => null;

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

        string sessionId = context.Request.SessionId.Value.ToString();

        foreach (VKKnowledgeLifecycleEntry entry in state.InjectedEntries)
        {
            VKResult recordResult = await _usageStore.RecordUsageAsync(
                sessionId,
                state.CurrentTurn,
                entry.Knowledge.Id.Value.ToString(),
                entry.Knowledge.Tag,
                cancellationToken).ConfigureAwait(false); // [CS.03]

            if (!recordResult.IsSuccess)
            {
                CorpusLog.FailedToRecordUsage(
                    _logger,
                    entry.Knowledge.Id.Value.ToString(),
                    sessionId,
                    recordResult.FirstError.Description);
            }
        }

        return VKResult.Success();
    }
}
