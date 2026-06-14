using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.AI.Corpus.Common.Models.Internal;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Corpus.Filtering.Internal;

/// <summary>
/// Pipeline stage that evaluates and filters all knowledge/corpus candidates currently in the context.
/// Implements <see cref="IVKPsycheBeforePipelineStage"/>.
/// Follows BB.01 / AP.03.
/// </summary>
internal sealed class DefaultFilteringStage : IVKPsycheBeforePipelineStage
{
    private readonly IVKStaticKnowledgeLifecycleStore _staticKnowledgeLifecycleStore;
    private readonly IReadOnlyList<IVKKnowledgeLifecycleFilter> _knowledgeLifecyclefilters;
    private readonly IVKKnowledgeInjectionStore _knowledgeInjectionStore;
    private readonly IVKEchoStore _echoStore;
    private readonly VKFilteringOptions _filteringOptions;
    private readonly VKAICorpusOptions _corpusOptions;
    private readonly VKKnowledgeOptions _knowledgeOptions;
    private readonly ILogger<DefaultFilteringStage> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="DefaultFilteringStage"/>.
    /// </summary>
    public DefaultFilteringStage(
        IVKStaticKnowledgeLifecycleStore staticKnowledgeLifecycleStore,
        IEnumerable<IVKKnowledgeLifecycleFilter> knowledgeLifecyclefilters,
        IVKKnowledgeInjectionStore knowledgeInjectionStore,
        IVKEchoStore echoStore,
        IOptions<VKFilteringOptions> filteringOptions,
        IOptions<VKAICorpusOptions> corpusOptions,
        IOptions<VKKnowledgeOptions> knowledgeOptions,
        ILogger<DefaultFilteringStage> logger)
    {
        _staticKnowledgeLifecycleStore = VKGuard.NotNull(staticKnowledgeLifecycleStore);
        _knowledgeLifecyclefilters = [.. VKGuard.NotNull(knowledgeLifecyclefilters)];
        _knowledgeInjectionStore = VKGuard.NotNull(knowledgeInjectionStore);
        _echoStore = VKGuard.NotNull(echoStore);
        _filteringOptions = VKGuard.NotNull(filteringOptions?.Value);
        _corpusOptions = VKGuard.NotNull(corpusOptions?.Value);
        _knowledgeOptions = VKGuard.NotNull(knowledgeOptions?.Value);
        _logger = VKGuard.NotNull(logger);
    }

    /// <inheritdoc />
    public VKStageSchedule Schedule => VKPsychePipelineScheduler.Before.CorpusFiltering;

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
        VKResult<IReadOnlyCollection<VKEchoTrace>> historyResult = await _echoStore.GetHistoryAsync(context.Request.SessionId, cancellationToken).ConfigureAwait(false); // [CS.03]
        int currentTurn = 1;
        if (historyResult.IsSuccess)
        {
            currentTurn = historyResult.Value.Count(e => e.Role == VKChatRole.User) + 1;
        }

        // Retrieve the historical usage context
        VKResult<IReadOnlyCollection<VKKnowledgeInjection>> historyResultData = await _knowledgeInjectionStore.GetInjectionsAsync(
            context.Request.SessionId,
            cancellationToken).ConfigureAwait(false); // [CS.03]
        if (!historyResultData.IsSuccess)
        {
            return VKResult.Failure(historyResultData.FirstError);
        }

        HashSet<string> injectedTags = [];
        Dictionary<string, int> usageCounts = [];
        Dictionary<string, int> lastInjectedTurns = [];

        foreach (VKKnowledgeInjection record in historyResultData.Value)
        {
            if (!string.IsNullOrWhiteSpace(record.GroupId))
            {
                injectedTags.Add(record.GroupId);
                usageCounts[record.GroupId] = usageCounts.GetValueOrDefault(record.GroupId) + 1;
                usageCounts[$"group:{record.GroupId}"] = usageCounts.GetValueOrDefault($"group:{record.GroupId}") + 1;
                lastInjectedTurns[record.GroupId] = record.InjectedTurn;
            }

            if (!record.KnowledgeId.IsEmpty)
            {
                string entryIdStr = record.KnowledgeId.Value.ToString();
                injectedTags.Add(entryIdStr);
                usageCounts[entryIdStr] = usageCounts.GetValueOrDefault(entryIdStr) + 1;
                lastInjectedTurns[entryIdStr] = record.InjectedTurn;
            }
        }

        VKCorpusContext corpusContext = new()
        {
            SessionId = context.Request.SessionId,
            CurrentTurn = currentTurn,
            InjectedTags = injectedTags,
            UsageCounts = usageCounts,
            LastInjectedTurns = lastInjectedTurns
        };

        // Extract scan texts based on KeywordScanDepth from Psyche knowledge options
        int scanDepth = _knowledgeOptions.KeywordScanDepth;
        var scanTexts = new List<string>();

        if (historyResult.IsSuccess && historyResult.Value.Count > 0 && scanDepth != 0)
        {
            var echoes = historyResult.Value
                .OrderBy(e => e.Timestamp)
                .ToList();

            IEnumerable<VKEchoTrace> targetEchoes;
            if (scanDepth == -1)
            {
                targetEchoes = echoes;
            }
            else
            {
                int userCount = 0;
                int startIndex = 0;
                for (int i = echoes.Count - 1; i >= 0; i--)
                {
                    if (echoes[i].Role == VKChatRole.User)
                    {
                        userCount++;
                        if (userCount > scanDepth)
                        {
                            startIndex = i + 1;
                            break;
                        }
                    }
                }
                targetEchoes = echoes.Skip(startIndex);
            }

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

        // Enrich context with request-specific arguments
        VKCorpusArgs? corpusArgs = context.Args<VKCorpusArgs>();
        corpusContext = corpusContext with
        {
            PersonaId = context.Request.PersonaId.Value.ToString(),
            UserSegment = corpusArgs?.UserSegment,
            StateValues = corpusArgs?.StateValues ?? new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase),
            UnlockedSecrets = corpusArgs?.UnlockedSecrets ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase),
            ScanTexts = scanTexts
        };

        // Resolve candidates from candidates state
        var candidateState = context.State<VKKnowledgeCandidatesState>();
        var candidatesList = candidateState?.Candidates ?? new List<VKKnowledgeEntry>();

        // Resolve recalled entries from Retrieval stage
        var recalledEntries = context.State<RecalledKnowledgeLifecycleState>()?.RecalledEntries ?? [];
        var recalledMap = recalledEntries.ToDictionary(e => e.Knowledge.Id);

        // Collect all IDs that need to be resolved statically
        var staticIds = new List<VKKnowledgeId>();
        foreach (var knowledge in candidatesList)
        {
            if (!recalledMap.ContainsKey(knowledge.Id))
            {
                staticIds.Add(knowledge.Id);
            }
        }

        // Batch fetch static rules in one go
        var staticRulesMap = _staticKnowledgeLifecycleStore.GetLifecycleEntries(staticIds);

        // Map candidate entries to VKKnowledgeLifecycleEntry
        var candidateEntries = new List<VKKnowledgeLifecycleEntry>();
        foreach (var knowledge in candidatesList)
        {
            if (recalledMap.TryGetValue(knowledge.Id, out var recalledEntry))
            {
                candidateEntries.Add(recalledEntry);
            }
            else
            {
                if (staticRulesMap.TryGetValue(knowledge.Id, out var staticEntry))
                {
                    candidateEntries.Add(staticEntry with { Knowledge = knowledge });
                }
                else
                {
                    candidateEntries.Add(new VKKnowledgeLifecycleEntry
                    {
                        Knowledge = knowledge,
                        Lifecycle = new VKKnowledgeLifecycle()
                    });
                }
            }
        }

        // Sort candidate entries: higher exclusive weight is evaluated first
        var sortedCandidates = candidateEntries
            .OrderByDescending(e => e.Lifecycle.ExclusiveWeight)
            .ToList();

        List<VKKnowledgeLifecycleEntry> passedEntries = [];
        HashSet<string> currentTurnInjectedTags = [.. corpusContext.InjectedTags];

        // Evaluate candidate entries through the filter chain
        foreach (VKKnowledgeLifecycleEntry entry in sortedCandidates)
        {
            bool keep = true;
            foreach (IVKKnowledgeLifecycleFilter filter in _knowledgeLifecyclefilters)
            {
                VKCorpusContext tempContext = corpusContext with { InjectedTags = currentTurnInjectedTags };
                VKResult<VKFilterVerdict> filterResult = await filter.EvaluateAsync(entry, tempContext, cancellationToken).ConfigureAwait(false); // [CS.03]
                if (!filterResult.IsSuccess)
                {
                    return VKResult.Failure(filterResult.FirstError);
                }

                if (filterResult.Value == VKFilterVerdict.ForceKeep)
                {
                    keep = true;
                    break;
                }

                if (filterResult.Value == VKFilterVerdict.Reject)
                {
                    keep = false;
                    break;
                }
            }

            if (keep)
            {
                passedEntries.Add(entry);
                currentTurnInjectedTags.Add(entry.Knowledge.Id.Value.ToString());
                if (!string.IsNullOrEmpty(entry.Knowledge.XmlTag))
                {
                    currentTurnInjectedTags.Add(entry.Knowledge.XmlTag);
                }
            }
        }

        // Apply filtering results directly back to the candidates state list
        if (candidateState != null)
        {
            candidateState.Candidates.Clear();
            candidateState.Candidates.AddRange(passedEntries.Select(e => e.Knowledge));
        }

        // Propagate state downstream for usage recording
        context.SetState(new CorpusInjectionState(passedEntries, currentTurn));

        return VKResult.Success();
    }
}
