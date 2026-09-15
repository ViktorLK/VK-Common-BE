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
            nonHistoryTokens += context.UserEchoTrace?.TokenCount
                ?? _tokenCounter.CountTokens(context.Request.UserInput);
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

        int remainingHistoryBudget = availablePromptBudget - nonHistoryTokens;

        if (context.Echoes.Count == 0)
        {
            return Task.FromResult(VKResult.Success());
        }

        // 4. Resolve Echo options and group dialogue history into turns (newest to oldest)
        var echoOptions = context.Args<VKEchoArgs>().Merge(_echoOptions);
        int minRetainedTurns = Math.Max(0, echoOptions.MinRetainedTurns);

        var echoesAscending = context.Echoes.OrderBy(e => e.TurnIndex).ToList();
        var turns = GroupIntoTurns(echoesAscending);

        // 5. Enforce MinRetainedTurns guarantee
        int requiredTurnsCount = Math.Min(minRetainedTurns, turns.Count);
        int requiredTurnsTokens = 0;
        for (int t = 0; t < requiredTurnsCount; t++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            requiredTurnsTokens += GetTurnTokens(turns[t]);
        }

        if (nonHistoryTokens + requiredTurnsTokens > availablePromptBudget)
        {
            _logger.ContextBudgetExceeded(context.Request.SessionId, nonHistoryTokens + requiredTurnsTokens, availablePromptBudget);
            return Task.FromResult(VKResult.Failure(
                VKWeavingErrors.ContextBudgetExceeded(nonHistoryTokens + requiredTurnsTokens, availablePromptBudget)));
        }

        // 6. Retain required turns, then fit as many older turns as possible
        var retainedTurns = new List<List<VKEchoFragment>>(turns.Count);
        int activeHistoryTokens = requiredTurnsTokens;

        for (int t = 0; t < requiredTurnsCount; t++)
        {
            retainedTurns.Add(turns[t]);
        }

        for (int t = requiredTurnsCount; t < turns.Count; t++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var turn = turns[t];
            int turnTokens = GetTurnTokens(turn);

            if (activeHistoryTokens + turnTokens <= remainingHistoryBudget)
            {
                retainedTurns.Add(turn);
                activeHistoryTokens += turnTokens;
            }
            else
            {
                // Evict this turn and all older turns
                var evictedState = context.State<VKPsycheEvictedState>() ?? new VKPsycheEvictedState();
                context.SetState(evictedState);

                for (int k = t; k < turns.Count; k++)
                {
                    foreach (var echo in turns[k])
                    {
                        evictedState.Add(echo);
                    }
                }
                break;
            }
        }

        // 7. Flatten retained turns, restore chronological order (oldest first), and update context
        var retainedEchoes = retainedTurns
            .SelectMany(t => t)
            .OrderBy(e => e.TurnIndex)
            .ToList();

        context.SetEchoes(retainedEchoes);

        var finalEvictedState = context.State<VKPsycheEvictedState>();
        int evictedCount = finalEvictedState?.Evicted.Count ?? 0;
        if (evictedCount > 0)
        {
            _logger.WeavingTruncated(context.Request.SessionId, remainingHistoryBudget, activeHistoryTokens, evictedCount);
            WeavingDiagnostics.RecordTruncation(evictedCount, "Truncate", remainingHistoryBudget);
        }

        return Task.FromResult(VKResult.Success()); // [CS.01]
    }

    private int GetTurnTokens(List<VKEchoFragment> turn)
    {
        int sum = 0;
        foreach (var echo in turn)
        {
            sum += echo.TokenCount > 0
                ? echo.TokenCount
                : (!string.IsNullOrWhiteSpace(echo.Content) ? _tokenCounter.CountTokens(echo.Content) : 0);
        }
        return sum;
    }

    private static List<List<VKEchoFragment>> GroupIntoTurns(List<VKEchoFragment> echoes)
    {
        var turns = new List<List<VKEchoFragment>>();
        if (echoes.Count == 0)
        {
            return turns;
        }

        var currentTurn = new List<VKEchoFragment>();

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
}
