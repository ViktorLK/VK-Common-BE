using System.Diagnostics;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VK.Blocks.AI.Corpus.Common.Models.Internal;
using VK.Blocks.AI.Corpus.Diagnostics.Internal;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Corpus.Gathering.Internal;

/// <summary>
/// Pipeline stage that dynamically recalls candidate knowledge lifecycle entries and injects them into the context.
/// Implements <see cref="IVKPsycheBeforePipelineStage"/>.
/// Follows BB.01 / AP.03.
/// </summary>
internal sealed class DefaultGatheringStage : IVKPsycheBeforePipelineStage
{
    private readonly IVKRecallKnowledgeLifecycleStore _recallStore;
    private readonly VKAICorpusOptions _corpusOptions;
    private readonly ILogger<DefaultGatheringStage> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="DefaultGatheringStage"/>.
    /// </summary>
    public DefaultGatheringStage(
        IVKRecallKnowledgeLifecycleStore recallStore,
        Microsoft.Extensions.Options.IOptions<VKAICorpusOptions> corpusOptions,
        ILogger<DefaultGatheringStage> logger)
    {
        _recallStore = VKGuard.NotNull(recallStore);
        _corpusOptions = VKGuard.NotNull(corpusOptions?.Value);
        _logger = VKGuard.NotNull(logger);
    }

    /// <inheritdoc />
    public VKStageSchedule Schedule => VKPsychePipelineScheduler.Before.CorpusGathering;

    /// <inheritdoc />
    public bool IsActive => _corpusOptions.Enabled;

    /// <inheritdoc />
    public async Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken cancellationToken)
    {
        VKGuard.NotNull(context);

        if (!IsActive)
        {
            return VKResult.Success();
        }

        Stopwatch stopwatch = Stopwatch.StartNew();

        // Calculate the current turn by counting user messages in the dialogue history.
        // Gathering stage does not require precise turn calculation; it is calculated correctly in the filtering stage.
        int currentTurn = 1;
        // In retrieval, we only need to pass the basic context to GetEntriesAsync
        VKCorpusContext corpusContext = new()
        {
            SessionId = context.Request.SessionId,
            CurrentTurn = currentTurn,
            PersonaId = context.Request.PersonaId.Value.ToString()
        };

        // Fetch dynamic entries from the recall database
        VKResult<IReadOnlyList<VKKnowledgeLifecycleEntry>> recalledResult = await _recallStore.GetLifecycleEntriesAsync(corpusContext, cancellationToken).ConfigureAwait(false); // [CS.03]
        if (!recalledResult.IsSuccess)
        {
            return VKResult.Failure(recalledResult.FirstError);
        }

        var candidateState = context.State<VKKnowledgeCandidatesState>();
        if (candidateState == null)
        {
            candidateState = new VKKnowledgeCandidatesState();
            context.SetState(candidateState);
        }

        for (int i = 0; i < recalledResult.Value.Count; i++)
        {
            candidateState.Candidates.Add(recalledResult.Value[i].Knowledge);
        }

        // Store the state so the Filtering stage knows their options
        context.SetState(new RecalledKnowledgeLifecycleState(recalledResult.Value));

        stopwatch.Stop();
        CorpusDiagnostics.RecordGathering(context.Request.SessionId.Value.ToString(), recalledResult.Value.Count, stopwatch.Elapsed.TotalMilliseconds);
        CorpusLog.GatheringCompleted(_logger, recalledResult.Value.Count, context.Request.SessionId.Value.ToString());

        return VKResult.Success();
    }
}
