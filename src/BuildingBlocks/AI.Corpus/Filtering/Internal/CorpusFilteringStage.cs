using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.AI.Corpus;
using VK.Blocks.AI.Corpus.KnowledgeSourcing.Internal;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Corpus.Filtering.Internal;

/// <summary>
/// Pipeline stage that evaluates and filters all knowledge/corpus candidates currently in the context.
/// Implements <see cref="IVKPsycheBeforePipelineStage"/>.
/// Follows BB.01 / AP.03.
/// </summary>
internal sealed class CorpusFilteringStage : IVKPsycheBeforePipelineStage
{
    private readonly IVKStaticKnowledgeLifecycleStore _staticCorpusStore;
    private readonly IReadOnlyList<IVKKnowledgeLifecycleFilter> _filters;
    private readonly IVKKnowledgeUsageStore _usageStore;
    private readonly IVKEchoStore _echoStore;
    private readonly VKFilteringOptions _filteringOptions;
    private readonly VKCorpusOptions _corpusOptions;
    private readonly VKKnowledgeOptions _knowledgeOptions;
    private readonly ILogger<CorpusFilteringStage> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="CorpusFilteringStage"/>.
    /// </summary>
    public CorpusFilteringStage(
        IVKStaticKnowledgeLifecycleStore staticCorpusStore,
        IEnumerable<IVKKnowledgeLifecycleFilter> filters,
        IVKKnowledgeUsageStore usageStore,
        IVKEchoStore echoStore,
        IOptions<VKFilteringOptions> filteringOptions,
        IOptions<VKCorpusOptions> corpusOptions,
        IOptions<VKKnowledgeOptions> knowledgeOptions,
        ILogger<CorpusFilteringStage> logger)
    {
        _staticCorpusStore = VKGuard.NotNull(staticCorpusStore);
        _filters = VKGuard.NotNull(filters).ToList();
        _usageStore = VKGuard.NotNull(usageStore);
        _echoStore = VKGuard.NotNull(echoStore);
        _filteringOptions = VKGuard.NotNull(filteringOptions?.Value);
        _corpusOptions = VKGuard.NotNull(corpusOptions?.Value);
        _knowledgeOptions = VKGuard.NotNull(knowledgeOptions?.Value);
        _logger = VKGuard.NotNull(logger);
    }

    /// <inheritdoc />
    public int StageOrder => VKPsychePipelineScheduler.Before.CorpusFiltering.Order;

    /// <inheritdoc />
    public bool IsActive => _corpusOptions.Enabled;

    /// <inheritdoc />
    public bool IsParallel => VKPsychePipelineScheduler.Before.CorpusFiltering.IsParallel;

    /// <inheritdoc />
    public int? ParallelGroup => VKPsychePipelineScheduler.Before.CorpusFiltering.ParallelGroup;

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
        VKResult<IReadOnlyCollection<VKEchoTrace>> historyResult = await _echoStore.GetHistoryAsync(context.Request.SessionId, cancellationToken).ConfigureAwait(false); // [CS.03]
        int currentTurn = 1;
        if (historyResult.IsSuccess)
        {
            currentTurn = historyResult.Value.Count(e => e.Role == VKChatRole.User) + 1;
        }

        // Retrieve the historical usage context
        VKResult<VKCorpusContext> usageContextResult = await _usageStore.GetContextAsync(sessionId, currentTurn, cancellationToken).ConfigureAwait(false); // [CS.03]
        if (!usageContextResult.IsSuccess)
        {
            return VKResult.Failure(usageContextResult.FirstError);
        }

        VKCorpusContext corpusContext = usageContextResult.Value;

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

        // Extract pre-matched knowledge fragments from the context and remove them temporarily
        var existingFragments = context.Fragments;
        var knowledgeFragments = existingFragments
            .Where(f => f.TierType == VKPromptTierType.Knowledge && f.Metadata is VKKnowledgeEntry)
            .ToList();
        var remainingFragments = existingFragments
            .Where(f => !(f.TierType == VKPromptTierType.Knowledge && f.Metadata is VKKnowledgeEntry))
            .ToList();
        context.SetFragments(remainingFragments);

        // Resolve recalled entries from Retrieval stage
        var recalledEntries = context.State<KnowledgeSourcingState>()?.RecalledEntries ?? [];
        var recalledMap = recalledEntries.ToDictionary(e => e.Knowledge.Id);

        // Collect all IDs that need to be resolved statically
        var staticIds = new List<VKKnowledgeId>();
        foreach (var fragment in knowledgeFragments)
        {
            var knowledge = (VKKnowledgeEntry)fragment.Metadata;
            if (!recalledMap.ContainsKey(knowledge.Id))
            {
                staticIds.Add(knowledge.Id);
            }
        }

        // Batch fetch static rules in one go
        var staticRulesMap = _staticCorpusStore.GetLifecycleEntries(staticIds);

        // Map candidate fragments to VKKnowledgeLifecycleEntry
        var candidateEntries = new List<VKKnowledgeLifecycleEntry>();
        foreach (var fragment in knowledgeFragments)
        {
            var knowledge = (VKKnowledgeEntry)fragment.Metadata;
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

        // Sort candidate entries: higher priority and higher exclusive weight are evaluated first
        var sortedCandidates = candidateEntries
            .OrderByDescending(e => e.Knowledge.Segment?.Priority ?? 0)
            .ThenByDescending(e => e.Lifecycle.ExclusiveWeight)
            .ToList();

        List<VKKnowledgeLifecycleEntry> passedEntries = [];
        HashSet<string> currentTurnInjectedTags = [.. corpusContext.InjectedTags];

        // Evaluate candidate entries through the filter chain
        foreach (VKKnowledgeLifecycleEntry entry in sortedCandidates)
        {
            bool keep = true;
            foreach (IVKKnowledgeLifecycleFilter filter in _filters)
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
                if (!string.IsNullOrEmpty(entry.Knowledge.Tag))
                {
                    currentTurnInjectedTags.Add(entry.Knowledge.Tag);
                }
            }
        }

        // Add the passed entries to the prompt context as knowledge fragments
        int baseRenderOrder = VKPsychePipelineScheduler.Before.Knowledge.Order - 5;
        for (int i = 0; i < passedEntries.Count; i++)
        {
            VKKnowledgeLifecycleEntry entry = passedEntries[i];
            context.AddFragment(new VKPromptFragment
            {
                TierType = VKPromptTierType.Knowledge,
                RenderOrder = baseRenderOrder + i,
                Metadata = entry.Knowledge,
                Segment = entry.Knowledge.Segment
            });
        }

        // Propagate state downstream for usage recording
        context.SetState(new CorpusInjectionState(passedEntries, currentTurn));

        return VKResult.Success();
    }
}
