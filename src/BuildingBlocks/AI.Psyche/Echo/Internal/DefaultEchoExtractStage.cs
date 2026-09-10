using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VK.Blocks.AI.Psyche.Echo.Diagnostics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Echo.Internal;

/// <summary>
/// Default implementation of the dialogue echo stage.
/// Retains and sliding-window trims short-term dialogue history using a 2-phase DDD retrieval model:
/// Phase 1: Queries lightweight metadata (VKEchoMetadata) to compute token/turn budgets in memory.
/// Phase 2: Fetches full dialogue content (VKEchoTrace) only for the retained message IDs.
/// Follows AP.01 (sealed class default) and CS.03.
/// </summary>
internal sealed class DefaultEchoExtractStage : IVKPsychePipelineStage
{
    private readonly IVKEchoStore _echoStore;
    private readonly IVKPsycheSessionRepository _sessionRepository;
    private readonly IVKEchoRenderer _echoRenderer;
    private readonly VKEchoOptions _echoOptions;
    private readonly VKWeavingOptions _weavingOptions;
    private readonly ILogger<DefaultEchoExtractStage> _logger;

    public DefaultEchoExtractStage(
        IVKEchoStore echoStore,
        IVKPsycheSessionRepository sessionRepository,
        IVKEchoRenderer echoRenderer,
        VKEchoOptions echoOptions,
        VKWeavingOptions weavingOptions,
        ILogger<DefaultEchoExtractStage> logger)
    {
        _echoStore = VKGuard.NotNull(echoStore);
        _sessionRepository = VKGuard.NotNull(sessionRepository);
        _echoRenderer = VKGuard.NotNull(echoRenderer);
        _echoOptions = VKGuard.NotNull(echoOptions);
        _weavingOptions = VKGuard.NotNull(weavingOptions);
        _logger = VKGuard.NotNull(logger);
    }

    public VKPipelineSchedule Schedule => VKPsychePipelineScheduler.Before.PsycheEcho;
    public bool IsActive => _echoOptions.Enabled;

    [VKTrace("psyche.stage.echo_extract")]
    public async Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(context);

        var echoOptions = context.Args<VKEchoArgs>().Merge(_echoOptions);
        if (!echoOptions.Enabled)
        {
            return VKResult.Success();
        }

        var session = context.State<VKSessionThread>();
        if (session is null)
        {
            return VKResult.Success();
        }

        // 1. Phase 1: Fetch lightweight metadata (supports Continuous multi-level parent ancestry tracing)
        var allMetas = new List<VKEchoMetadata>();
        var currentSessionId = (VKSessionId?)session.Id;
        var mode = session.Mode;

        var visitedSessions = new HashSet<VKSessionId>();

        while (currentSessionId.HasValue && visitedSessions.Add(currentSessionId.Value))
        {
            var metadataResult = await _echoStore.GetMetadataAsync(currentSessionId.Value, cancellationToken).ConfigureAwait(false);
            if (metadataResult.IsSuccess && metadataResult.Value.Count > 0)
            {
                // Prepend parent echoes before child echoes
                allMetas.InsertRange(0, metadataResult.Value);
            }

            // Only trace parent dynamically if mode is Continuous
            if (mode == VKSessionMode.Continuous)
            {
                if (session.Id == currentSessionId.Value)
                {
                    currentSessionId = session.ParentSessionId;
                }
                else
                {
                    var sessionResult = await _sessionRepository.FindByIdAsync(currentSessionId.Value, cancellationToken).ConfigureAwait(false);
                    currentSessionId = sessionResult.IsSuccess ? sessionResult.Value.ParentSessionId : null;
                }
            }
            else
            {
                break;
            }
        }

        if (allMetas.Count == 0)
        {
            return VKResult.Success();
        }

        // 2. Apply sliding window constraint (MaxWindowSize) if defined in request overrides or options
        var maxWindowSize = echoOptions.MaxWindowSize;
        if (maxWindowSize.HasValue && maxWindowSize.Value > 0 && allMetas.Count > maxWindowSize.Value)
        {
            allMetas = [.. allMetas.Skip(allMetas.Count - maxWindowSize.Value)];
        }

        // 3. Filter System Messages if disabled
        if (!echoOptions.IncludeSystemMessages)
        {
            allMetas.RemoveAll(e => e.Role == VKChatRole.System);
        }

        if (allMetas.Count == 0)
        {
            return VKResult.Success();
        }

        // 4. Resolve Effective Token Budget from Weaving budget or resolved model metadata
        var modelMetadata = context.State<VKAIModelMetadata>();
        var configuredBudget = context.Args<VKWeavingArgs>()?.MaxContextBudget ?? _weavingOptions.MaxContextBudget;
        var totalLimit = configuredBudget ?? modelMetadata?.ContextWindowSize;

        int effectiveBudget = int.MaxValue;
        if (echoOptions.MaxTokens.HasValue && echoOptions.MaxTokens.Value > 0)
        {
            effectiveBudget = echoOptions.MaxTokens.Value;
        }

        if (totalLimit.HasValue)
        {
            int dynamicLimit = (int)(totalLimit.Value * echoOptions.TokenBudgetRatio);
            effectiveBudget = Math.Min(effectiveBudget, dynamicLimit);
        }

        // 5. Trim dialogue metadata in-memory (from oldest to newest)
        var retainedMetas = new List<VKEchoMetadata>();

        if (echoOptions.PruneUnit == VKEchoPruneUnit.Turn)
        {
            // Prune by whole Turns (alternating user dialog steps)
            var turns = GroupIntoTurns([.. allMetas]);
            int currentTokensSum = 0;
            int retainedTurnsCount = 0;
            var maxTurns = echoOptions.MaxTurns;

            foreach (var turn in turns)
            {
                if (maxTurns.HasValue && retainedTurnsCount >= maxTurns.Value)
                {
                    break;
                }

                int turnTokens = turn.Sum(GetMetaTokens);
                if (currentTokensSum + turnTokens <= effectiveBudget)
                {
                    retainedMetas.InsertRange(0, turn); // Maintain oldest-first chronological order and intra-turn order
                    currentTokensSum += turnTokens;
                    retainedTurnsCount++;
                }
                else
                {
                    break; // Over budget: drop remaining oldest turns
                }
            }
        }
        else
        {
            // Prune message-by-message
            int currentTokensSum = 0;

            for (int i = allMetas.Count - 1; i >= 0; i--)
            {
                var item = allMetas[i];
                int itemTokens = GetMetaTokens(item);

                if (currentTokensSum + itemTokens <= effectiveBudget)
                {
                    retainedMetas.Insert(0, item); // Prepend to preserve oldest-first
                    currentTokensSum += itemTokens;
                }
                else
                {
                    break; // Over budget: drop remaining oldest messages
                }
            }
        }

        if (retainedMetas.Count == 0)
        {
            return VKResult.Success();
        }

        // 6. Phase 2: Fetch full dialogue traces for ONLY the retained message IDs
        var targetIds = retainedMetas.Select(m => m.Id).ToList();
        var tracesResult = await _echoStore.GetTracesByIdsAsync(targetIds, cancellationToken).ConfigureAwait(false);
        if (tracesResult.IsFailure)
        {
            return VKResult.Failure(tracesResult.Errors);
        }

        // Defensive ordering: align traces strictly with Phase 1 chronological order
        var traceMap = tracesResult.Value.ToDictionary(t => t.Id);
        var retained = retainedMetas
            .Where(m => traceMap.ContainsKey(m.Id))
            .Select(m => traceMap[m.Id])
            .ToList();

        context.SetState(retained);

        for (var i = 0; i < retained.Count; i++)
        {
            var echo = retained[i];
            var renderedContent = _echoRenderer.Render(echo, context);
            context.AddEcho(new VKEchoFragment
            {
                Content = renderedContent,
                Role = echo.Role,
                TurnIndex = i,
                TokenCount = echo.TokenCount
            });
        }

        var allMetasCount = allMetas.Count;
        var retainedCount = retained.Count;
        var trimmedCount = Math.Max(0, allMetasCount - retainedCount);
        _logger.EchoTrimmed(session.Id, allMetasCount, retainedCount);

        Activity.Current?.SetPsycheEchoCount(retainedCount, trimmedCount);
        if (retainedCount > 0)
        {
            EchoDiagnostics.RecordActiveEchoes(retainedCount, "EchoExtract");
        }
        if (trimmedCount > 0)
        {
            EchoDiagnostics.RecordTrimmedEchoes(trimmedCount, "EchoExtract");
        }

        return VKResult.Success();
    }

    /// <summary>
    /// Groups dialogue metadata into turn exchanges (from newest to oldest).
    /// </summary>
    private static List<List<VKEchoMetadata>> GroupIntoTurns(IReadOnlyList<VKEchoMetadata> echoes)
    {
        var turns = new List<List<VKEchoMetadata>>();
        if (echoes.Count == 0)
        {
            return turns;
        }

        var currentTurn = new List<VKEchoMetadata>();

        // Walk backwards from latest to oldest
        for (int i = echoes.Count - 1; i >= 0; i--)
        {
            var msg = echoes[i];
            currentTurn.Insert(0, msg);

            // A User turn marker completes a conversational turn exchange
            if (msg.Role == VKChatRole.User)
            {
                turns.Add(currentTurn);
                currentTurn = [];
            }
        }

        if (currentTurn.Count > 0)
        {
            turns.Add(currentTurn);
        }

        return turns;
    }

    private static int GetMetaTokens(VKEchoMetadata meta)
    {
        return meta.TokenCount > 0 ? meta.TokenCount : 10;
    }
}
