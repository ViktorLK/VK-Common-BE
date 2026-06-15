using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Knowledge.Internal;

internal sealed class DefaultKnowledgeStage : IVKPsycheBeforePipelineStage
{
    private readonly VKKnowledgeOptions _options;
    private readonly IVKKnowledgeStore _store;
    private readonly VKWeavingOptions _weavingOptions;

    public VKStageSchedule Schedule => VKPsychePipelineScheduler.Before.PsycheKnowledge;
    public bool IsActive => _options.Enabled;

    public DefaultKnowledgeStage(
        IOptions<VKKnowledgeOptions> options,
        IVKKnowledgeStore store,
        IOptions<VKWeavingOptions> weavingOptions)
    {
        _options = VKGuard.NotNull(options).Value;
        _store = VKGuard.NotNull(store);
        _weavingOptions = VKGuard.NotNull(weavingOptions?.Value);
    }

    public async Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken ct)
    {
        VKGuard.NotNull(context);

        var disabledTiers = context.Args<VKWeavingArgs>()?.DisabledTiers ?? _weavingOptions.DisabledTiers;
        if (disabledTiers is not null && disabledTiers.Contains(VKPromptTierType.Knowledge))
        {
            return VKResult.Success();
        }

        if (context.Request.PersonaId.IsEmpty)
        {
            return VKResult.Failure(VKKnowledgeErrors.MissingPersona);
        }

        var knowledgeResult = await _store.GetRelevantEntriesAsync(context.Request.PersonaId, ct).ConfigureAwait(false); // [CS.03]
        if (knowledgeResult.IsFailure)
        {
            return VKResult.Failure(knowledgeResult.Errors); // [CS.01]
        }

        var candidateEntries = knowledgeResult.Value.Where(e => e.Segment.IsEnabled).ToList();

        // Separate constant entries from conditional keyword/regex entries
        var activeEntries = candidateEntries
            .Where(e => e.TriggerType == VKKnowledgeTriggerType.Constant)
            .ToList();

        var conditionalEntries = candidateEntries
            .Where(e => e.TriggerType != VKKnowledgeTriggerType.Constant)
            .ToList();

        // Fetch dialogue history from context fragments to scan for keyword matching
        var echoes = context.Fragments
            .Where(f => f.TierType == VKPromptTierType.Echo && f.Metadata is VKEchoTrace)
            .OrderBy(f => f.RenderOrder)
            .Select(f => (VKEchoTrace)f.Metadata!)
            .ToList();

        // Build the text list to scan based on KeywordScanDepth
        var scanTexts = new List<string>();

        if (_options.KeywordScanDepth != 0 && echoes.Count > 0)
        {
            var targetEchoes = _options.KeywordScanDepth == -1
                ? echoes
                : echoes.Skip(System.Math.Max(0, echoes.Count - _options.KeywordScanDepth));

            foreach (var echo in targetEchoes)
            {
                if (!string.IsNullOrWhiteSpace(echo.Content))
                {
                    scanTexts.Add(echo.Content);
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(context.Request.UserInput))
        {
            scanTexts.Add(context.Request.UserInput);
        }

        // Evaluate conditional knowledge entries
        if (scanTexts.Count > 0)
        {
            foreach (var entry in conditionalEntries)
            {
                var matcher = VKKnowledgeMatcher.GetMatcher(entry);
                if (scanTexts.Any(text => matcher(text)))
                {
                    activeEntries.Add(entry);
                }
            }
        }

        var candidateState = context.State<VKKnowledgeCandidatesState>();
        if (candidateState == null)
        {
            candidateState = new VKKnowledgeCandidatesState();
            context.SetState(candidateState);
        }
        candidateState.Candidates.AddRange(activeEntries);

        return VKResult.Success();
    }
}
