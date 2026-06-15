using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.AI.Corpus.Common.Models.Internal;
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

    /// <summary>
    /// Initializes a new instance of <see cref="DefaultGatheringStage"/>.
    /// </summary>
    public DefaultGatheringStage(IVKRecallKnowledgeLifecycleStore recallStore, Microsoft.Extensions.Options.IOptions<VKAICorpusOptions> corpusOptions)
    {
        _recallStore = VKGuard.NotNull(recallStore);
        _corpusOptions = VKGuard.NotNull(corpusOptions?.Value);
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

        // Calculate the current turn by counting user messages in the dialogue history
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

        return VKResult.Success();
    }
}
