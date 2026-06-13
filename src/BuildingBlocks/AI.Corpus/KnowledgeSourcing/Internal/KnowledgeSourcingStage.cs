using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Corpus.KnowledgeSourcing.Internal;

/// <summary>
/// Pipeline stage that dynamically recalls candidate knowledge lifecycle entries and injects them into the context.
/// Implements <see cref="IVKPsycheBeforePipelineStage"/>.
/// Follows BB.01 / AP.03.
/// </summary>
internal sealed class KnowledgeSourcingStage : IVKPsycheBeforePipelineStage
{
    private readonly IVKRecallKnowledgeLifecycleStore _corpusSource;
    private readonly VKCorpusOptions _corpusOptions;

    /// <summary>
    /// Initializes a new instance of <see cref="KnowledgeSourcingStage"/>.
    /// </summary>
    public KnowledgeSourcingStage(IVKRecallKnowledgeLifecycleStore corpusSource, Microsoft.Extensions.Options.IOptions<VKCorpusOptions> corpusOptions)
    {
        _corpusSource = VKGuard.NotNull(corpusSource);
        _corpusOptions = VKGuard.NotNull(corpusOptions?.Value);
    }

    /// <inheritdoc />
    public int StageOrder => VKPsychePipelineScheduler.Before.KnowledgeSourcing.Order;

    /// <inheritdoc />
    public bool IsActive => _corpusOptions.Enabled;

    /// <inheritdoc />
    public bool IsParallel => VKPsychePipelineScheduler.Before.KnowledgeSourcing.IsParallel;

    /// <inheritdoc />
    public int? ParallelGroup => VKPsychePipelineScheduler.Before.KnowledgeSourcing.ParallelGroup;

    /// <inheritdoc />
    public async Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken cancellationToken)
    {
        VKGuard.NotNull(context);

        if (!IsActive)
        {
            return VKResult.Success();
        }

        string sessionId = context.Request.SessionId.Value.ToString();

        // Calculate the current turn by counting user messages in the dialogue history
        int currentTurn = 1;
        // In retrieval, we only need to pass the basic context to GetEntriesAsync
        VKCorpusContext corpusContext = new()
        {
            SessionId = sessionId,
            CurrentTurn = currentTurn,
            PersonaId = context.Request.PersonaId.Value.ToString()
        };

        // Fetch dynamic entries from the recall database
        VKResult<IReadOnlyList<VKKnowledgeLifecycleEntry>> candidatesResult = await _corpusSource.GetLifecycleEntriesAsync(corpusContext, cancellationToken).ConfigureAwait(false); // [CS.03]
        if (!candidatesResult.IsSuccess)
        {
            return VKResult.Failure(candidatesResult.FirstError);
        }

        // Add the retrieved entries to the prompt context fragments
        int baseRenderOrder = VKPsychePipelineScheduler.Before.Knowledge.Order - 5;
        for (int i = 0; i < candidatesResult.Value.Count; i++)
        {
            VKKnowledgeLifecycleEntry entry = candidatesResult.Value[i];
            context.AddFragment(new VKPromptFragment
            {
                TierType = VKPromptTierType.Knowledge,
                RenderOrder = baseRenderOrder + i,
                Metadata = entry.Knowledge,
                Segment = entry.Knowledge.Segment
            });
        }

        // Store the state so the Filtering stage knows their options
        context.SetState(new KnowledgeSourcingState(candidatesResult.Value));

        return VKResult.Success();
    }
}
