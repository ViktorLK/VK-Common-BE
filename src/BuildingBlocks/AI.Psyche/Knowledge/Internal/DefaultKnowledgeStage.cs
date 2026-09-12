using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VK.Blocks.AI.Psyche.Knowledge.Diagnostics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Knowledge.Internal;

[VKTrace("psyche.stage.knowledge")]
internal sealed class DefaultKnowledgeStage : IVKPsychePipelineStage
{
    private readonly VKKnowledgeOptions _options;
    private readonly IVKPsycheKnowledgeRepository _knowledgeRepository;
    private readonly ILogger<DefaultKnowledgeStage> _logger;

    public VKPipelineSchedule Schedule => VKPsychePipelineScheduler.Before.PsycheKnowledge;
    public bool IsActive => _options.Enabled;

    public DefaultKnowledgeStage(
        VKKnowledgeOptions options,
        IVKPsycheKnowledgeRepository knowledgeRepository,
        ILogger<DefaultKnowledgeStage> logger)
    {
        _options = VKGuard.NotNull(options);
        _knowledgeRepository = VKGuard.NotNull(knowledgeRepository);
        _logger = VKGuard.NotNull(logger);
    }

    public async Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken ct)
    {
        VKGuard.NotNull(context);

        var isEnabled = context.Args<VKKnowledgeArgs>()?.Enabled ?? _options.Enabled;
        if (!isEnabled)
        {
            return VKResult.Success();
        }

        if (context.Request.KnowledgeIds.Count == 0)
        {
            return VKResult.Success();
        }

        var knowledgeResult = await _knowledgeRepository.ListByIdsAsync(context.Request.KnowledgeIds, ct).ConfigureAwait(false); // [CS.03]
        if (knowledgeResult.IsFailure)
        {
            return VKResult.Failure(knowledgeResult.Errors); // [CS.01]
        }

        var candidateEntries = knowledgeResult.Value;

        // Separate constant entries from conditional keyword/regex entries
        var activeEntries = candidateEntries
            .Where(e => e.TriggerType == VKKnowledgeTriggerType.Constant)
            .ToList();

        var conditionalEntries = candidateEntries
            .Where(e => e.TriggerType != VKKnowledgeTriggerType.Constant)
            .ToList();

        // Build texts to scan: current UserInput + recent historical Echoes
        var scanTexts = new List<string>();
        if (!string.IsNullOrWhiteSpace(context.Request.UserInput))
        {
            scanTexts.Add(context.Request.UserInput);
        }

        var scanDepth = context.Args<VKKnowledgeArgs>()?.KeywordScanDepth ?? _options.KeywordScanDepth;
        if (scanDepth > 0 && context.Echoes.Count > 0)
        {
            int userCount = 0;
            int startIndex = 0;

            for (int i = context.Echoes.Count - 1; i >= 0; i--)
            {
                if (context.Echoes[i].Role == VKChatRole.User)
                {
                    userCount++;
                    startIndex = i;
                    if (userCount >= scanDepth)
                    {
                        break;
                    }
                }
            }

            for (int i = startIndex; i < context.Echoes.Count; i++)
            {
                var content = context.Echoes[i].Content;
                if (!string.IsNullOrWhiteSpace(content))
                {
                    scanTexts.Add(content);
                }
            }
        }
        else if (scanDepth == -1 && context.Echoes.Count > 0)
        {
            foreach (var echo in context.Echoes)
            {
                if (!string.IsNullOrWhiteSpace(echo.Content))
                {
                    scanTexts.Add(echo.Content);
                }
            }
        }

        // Incremental scan across all target texts
        if (scanTexts.Count > 0 && conditionalEntries.Count > 0)
        {
            foreach (var entry in conditionalEntries)
            {
                var matcher = DefaultKnowledgeMatcher.GetMatcher(entry);
                if (scanTexts.Any(text => matcher(text)))
                {
                    if (!activeEntries.Contains(entry))
                    {
                        activeEntries.Add(entry);
                    }
                }
            }
        }

        var candidateState = context.State<VKKnowledgeCandidatesState>();
        if (candidateState is null)
        {
            candidateState = new VKKnowledgeCandidatesState();
            context.SetState(candidateState);
        }
        candidateState.Candidates.AddRange(activeEntries);

        Activity.Current?.SetPsycheKnowledgeCount(activeEntries.Count);
        if (activeEntries.Count > 0)
        {
            KnowledgeDiagnostics.RecordEntriesMatched(activeEntries.Count, "Knowledge", "Keyword+Constant");
        }
        _logger.KnowledgeMatched(activeEntries.Count, context.Request.SessionId, context.CorrelationId, 0);

        return VKResult.Success();
    }
}
