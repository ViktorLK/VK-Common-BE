using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VK.Blocks.AI.Psyche.Weaving.Diagnostics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Echo.Internal;

internal sealed class DefaultEchoTruncateTask : IVKWeavingPipelineTask
{
    private readonly IVKTokenCounter _tokenCounter;
    private readonly VKWeavingOptions _weavingOptions;
    private readonly VKEchoOptions _echoOptions;
    private readonly ILogger<DefaultEchoTruncateTask> _logger;

    public VKPipelineSchedule Schedule => new(VKWeavingTaskOrder.Truncate);

    public DefaultEchoTruncateTask(
        IVKTokenCounter tokenCounter,
        VKWeavingOptions weavingOptions,
        VKEchoOptions echoOptions,
        ILogger<DefaultEchoTruncateTask> logger)
    {
        _tokenCounter = VKGuard.NotNull(tokenCounter); // [AP.01]
        _weavingOptions = VKGuard.NotNull(weavingOptions); // [AP.01]
        _echoOptions = VKGuard.NotNull(echoOptions); // [AP.01]
        _logger = VKGuard.NotNull(logger); // [AP.01]
    }

    public Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(context); // [AP.01]

        // 1. Resolve Total Limit from Weaving budget or resolved model metadata
        var modelMetadata = context.State<VKAIModelMetadata>();
        var configuredBudget = context.Args<VKWeavingArgs>()?.MaxContextBudget ?? _weavingOptions.MaxContextBudget;
        var totalLimit = configuredBudget ?? modelMetadata?.ContextWindowSize;
        if (!totalLimit.HasValue)
        {
            return Task.FromResult(VKResult.Success()); // [CS.01]
        }

        // 2. Count tokens of all non-history segments (Segments across all tiers + UserInput)
        // Hot path reads precalculated TokenCount (0 tokenizer calls); Cold fallback calls _tokenCounter.
        int nonHistoryTokens = 0;
        foreach (var segment in context.Segments)
        {
            cancellationToken.ThrowIfCancellationRequested();
            nonHistoryTokens += segment.TokenCount > 0
                ? segment.TokenCount
                : (!string.IsNullOrWhiteSpace(segment.Content) ? _tokenCounter.CountTokens(segment.Content) : 0);
        }

        if (!string.IsNullOrWhiteSpace(context.Request.UserInput))
        {
            int userTokens = context.UserEchoTrace?.TokenCount is > 0
                ? context.UserEchoTrace.TokenCount
                : _tokenCounter.CountTokens(context.Request.UserInput);
            nonHistoryTokens += userTokens;
        }

        // 3. Compute available prompt budget after subtracting reserved response tokens
        var reservedResponse = context.Args<VKWeavingArgs>()?.ResponseReservedTokens ?? _weavingOptions.ResponseReservedTokens;
        int availablePromptBudget = Math.Max(0, totalLimit.Value - reservedResponse);

        // Fail-fast if non-history tokens alone already exceed the available prompt budget
        if (nonHistoryTokens > availablePromptBudget)
        {
            _logger.ContextBudgetExceeded(context.Request.SessionId, nonHistoryTokens, availablePromptBudget);
            return Task.FromResult(VKResult.Failure(VKWeavingErrors.ContextBudgetExceeded(nonHistoryTokens, availablePromptBudget)));
        }

        if (context.Echoes.Count == 0)
        {
            return Task.FromResult(VKResult.Success());
        }

        // 4. Resolve Echo options and effective history budget cap (MaxTokens defense)
        var echoOptions = context.Args<VKEchoArgs>().Merge(_echoOptions);
        int maxHistoryBudget = echoOptions.MaxTokens.HasValue && echoOptions.MaxTokens.Value > 0
            ? echoOptions.MaxTokens.Value
            : int.MaxValue;

        int remainingHistoryBudget = Math.Min(maxHistoryBudget, availablePromptBudget - nonHistoryTokens);

        var echoesAscending = context.Echoes.OrderBy(e => e.TurnIndex).ToList();

        // 5. Delegate truncation based on configured PruneUnit
        return echoOptions.PruneUnit == VKEchoPruneUnit.Message
            ? TruncateByMessages(context, echoesAscending, nonHistoryTokens, availablePromptBudget, remainingHistoryBudget, echoOptions, cancellationToken)
            : TruncateByTurns(context, echoesAscending, nonHistoryTokens, availablePromptBudget, remainingHistoryBudget, echoOptions, cancellationToken);
    }

    private Task<VKResult> TruncateByTurns(
        VKPsycheContext context,
        List<VKEchoFragment> echoesAscending,
        int nonHistoryTokens,
        int availablePromptBudget,
        int remainingHistoryBudget,
        VKEchoOptions echoOptions,
        CancellationToken cancellationToken)
    {
        var turns = EchoTurnGrouper.Group(echoesAscending, e => e.Role);
        int minRetainedTurns = Math.Max(0, echoOptions.MinRetainedTurns);

        // Enforce MinRetainedTurns guarantee
        int requiredTurnsCount = Math.Min(minRetainedTurns, turns.Count);
        int requiredTurnsTokens = 0;
        for (int t = 0; t < requiredTurnsCount; t++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            requiredTurnsTokens += GetTurnTokens(turns[t], cancellationToken);
        }

        if (nonHistoryTokens + requiredTurnsTokens > availablePromptBudget)
        {
            _logger.ContextBudgetExceeded(context.Request.SessionId, nonHistoryTokens + requiredTurnsTokens, availablePromptBudget);
            return Task.FromResult(VKResult.Failure(
                VKWeavingErrors.ContextBudgetExceeded(nonHistoryTokens + requiredTurnsTokens, availablePromptBudget)));
        }

        var retainedTurns = new List<List<VKEchoFragment>>(turns.Count);
        int activeHistoryTokens = requiredTurnsTokens;

        for (int t = 0; t < requiredTurnsCount; t++)
        {
            retainedTurns.Add(turns[t]);
        }

        int candidateTurnsLimit = echoOptions.MaxTurns.HasValue && echoOptions.MaxTurns.Value >= 0
            ? Math.Min(echoOptions.MaxTurns.Value, turns.Count)
            : turns.Count;

        for (int t = requiredTurnsCount; t < candidateTurnsLimit; t++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var turn = turns[t];
            int turnTokens = GetTurnTokens(turn, cancellationToken);

            if (activeHistoryTokens + turnTokens <= remainingHistoryBudget)
            {
                retainedTurns.Add(turn);
                activeHistoryTokens += turnTokens;
            }
            else
            {
                // Evict this turn and all older turns
                EvictTurns(context, turns, t);
                break;
            }
        }

        if (candidateTurnsLimit < turns.Count && retainedTurns.Count == candidateTurnsLimit)
        {
            EvictTurns(context, turns, candidateTurnsLimit);
        }

        // Flatten retained turns, restore chronological order (oldest first) without OrderBy/SelectMany allocations
        int totalRetainedCount = 0;
        for (int i = 0; i < retainedTurns.Count; i++)
        {
            totalRetainedCount += retainedTurns[i].Count;
        }

        var retainedEchoes = new List<VKEchoFragment>(totalRetainedCount);
        for (int i = retainedTurns.Count - 1; i >= 0; i--)
        {
            retainedEchoes.AddRange(retainedTurns[i]);
        }

        context.SetEchoes(retainedEchoes);

        RecordTruncationDiagnostics(context, remainingHistoryBudget, activeHistoryTokens);
        return Task.FromResult(VKResult.Success()); // [CS.01]
    }

    private Task<VKResult> TruncateByMessages(
        VKPsycheContext context,
        List<VKEchoFragment> echoesAscending,
        int nonHistoryTokens,
        int availablePromptBudget,
        int remainingHistoryBudget,
        VKEchoOptions echoOptions,
        CancellationToken cancellationToken)
    {
        // 1. Precalculate/backfill tokens for all messages
        var processedEchoes = new List<VKEchoFragment>(echoesAscending.Count);
        for (int i = 0; i < echoesAscending.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var echo = echoesAscending[i];
            int count = echo.TokenCount > 0
                ? echo.TokenCount
                : (!string.IsNullOrWhiteSpace(echo.Content) ? _tokenCounter.CountTokens(echo.Content) : 0);
            processedEchoes.Add(echo.TokenCount == count ? echo : echo with { TokenCount = count });
        }

        // 2. Identify required messages guaranteed by MinRetainedTurns (scanning backwards)
        int minRetainedTurns = Math.Max(0, echoOptions.MinRetainedTurns);
        int requiredStartIndex = processedEchoes.Count;
        if (minRetainedTurns > 0)
        {
            int userTurnsFound = 0;
            for (int i = processedEchoes.Count - 1; i >= 0; i--)
            {
                if (processedEchoes[i].Role == VKChatRole.User)
                {
                    userTurnsFound++;
                    if (userTurnsFound >= minRetainedTurns)
                    {
                        requiredStartIndex = i;
                        break;
                    }
                }
            }
            if (userTurnsFound < minRetainedTurns)
            {
                requiredStartIndex = 0;
            }
        }

        int requiredTokens = 0;
        for (int i = requiredStartIndex; i < processedEchoes.Count; i++)
        {
            requiredTokens += processedEchoes[i].TokenCount;
        }

        if (nonHistoryTokens + requiredTokens > availablePromptBudget)
        {
            _logger.ContextBudgetExceeded(context.Request.SessionId, nonHistoryTokens + requiredTokens, availablePromptBudget);
            return Task.FromResult(VKResult.Failure(
                VKWeavingErrors.ContextBudgetExceeded(nonHistoryTokens + requiredTokens, availablePromptBudget)));
        }

        // 3. Greedily retain older messages within remainingHistoryBudget and optional MaxWindowSize
        int activeHistoryTokens = requiredTokens;
        int retainedStartIndex = requiredStartIndex;

        int maxWindow = echoOptions.MaxWindowSize.HasValue && echoOptions.MaxWindowSize.Value > 0
            ? echoOptions.MaxWindowSize.Value
            : int.MaxValue;

        int retainedCount = processedEchoes.Count - requiredStartIndex;

        for (int i = requiredStartIndex - 1; i >= 0; i--)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (retainedCount >= maxWindow)
            {
                break;
            }

            var msg = processedEchoes[i];
            if (activeHistoryTokens + msg.TokenCount <= remainingHistoryBudget)
            {
                activeHistoryTokens += msg.TokenCount;
                retainedStartIndex = i;
                retainedCount++;
            }
            else
            {
                break;
            }
        }

        // Evict un-retained older messages (from index 0 to retainedStartIndex - 1)
        if (retainedStartIndex > 0)
        {
            var evictedState = context.State<VKPsycheEvictedState>() ?? new VKPsycheEvictedState();
            context.SetState(evictedState);
            for (int i = 0; i < retainedStartIndex; i++)
            {
                evictedState.Add(processedEchoes[i]);
            }
        }

        // Retain messages from retainedStartIndex to end (already in chronological order)
        var retainedEchoes = new List<VKEchoFragment>(processedEchoes.Count - retainedStartIndex);
        for (int i = retainedStartIndex; i < processedEchoes.Count; i++)
        {
            retainedEchoes.Add(processedEchoes[i]);
        }

        context.SetEchoes(retainedEchoes);

        RecordTruncationDiagnostics(context, remainingHistoryBudget, activeHistoryTokens);
        return Task.FromResult(VKResult.Success()); // [CS.01]
    }

    private int GetTurnTokens(List<VKEchoFragment> turn, CancellationToken cancellationToken)
    {
        int sum = 0;
        for (int i = 0; i < turn.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var echo = turn[i];
            int count = echo.TokenCount > 0
                ? echo.TokenCount
                : (!string.IsNullOrWhiteSpace(echo.Content) ? _tokenCounter.CountTokens(echo.Content) : 0);

            if (echo.TokenCount != count)
            {
                turn[i] = echo with { TokenCount = count };
            }

            sum += count;
        }
        return sum;
    }

    private static void EvictTurns(VKPsycheContext context, List<List<VKEchoFragment>> turns, int fromIndex)
    {
        var evictedState = context.State<VKPsycheEvictedState>() ?? new VKPsycheEvictedState();
        context.SetState(evictedState);

        for (int k = fromIndex; k < turns.Count; k++)
        {
            foreach (var echo in turns[k])
            {
                evictedState.Add(echo);
            }
        }
    }

    private void RecordTruncationDiagnostics(VKPsycheContext context, int remainingBudget, int activeHistoryTokens)
    {
        var finalEvictedState = context.State<VKPsycheEvictedState>();
        int evictedCount = finalEvictedState?.EvictedEchoes.Count ?? 0;
        if (evictedCount > 0)
        {
            _logger.WeavingTruncated(context.Request.SessionId, remainingBudget, activeHistoryTokens, evictedCount);
            WeavingDiagnostics.RecordTruncation(evictedCount, "Truncate", remainingBudget);
        }
    }
}
