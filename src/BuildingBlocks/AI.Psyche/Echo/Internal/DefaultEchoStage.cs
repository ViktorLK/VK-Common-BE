using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.AI.Psyche.Common.Internal;
using VK.Blocks.AI.Psyche.Echo.Diagnostics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Echo.Internal;

/// <summary>
/// Pipeline stage for interacting with Echo store and applying dialogue history pruning.
/// Implements AP.01 (sealed class default) and CS.03.
/// </summary>
internal sealed class DefaultEchoStage : IVKPsycheBeforePipelineStage
{
    private readonly IVKEchoStore _echoStore;
    private readonly IVKTokenCounter _tokenCounter;
    private readonly VKEchoOptions _echoOptions;
    private readonly VKWeavingOptions _weavingOptions;
    private readonly ILogger<DefaultEchoStage> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultEchoStage"/> class.
    /// </summary>
    public DefaultEchoStage(
        IVKEchoStore echoStore,
        IVKTokenCounter tokenCounter,
        IOptions<VKEchoOptions> echoOptions,
        IOptions<VKWeavingOptions> weavingOptions,
        ILogger<DefaultEchoStage> logger)
    {
        _echoStore = VKGuard.NotNull(echoStore);
        _tokenCounter = VKGuard.NotNull(tokenCounter);
        _echoOptions = VKGuard.NotNull(echoOptions?.Value);
        _weavingOptions = VKGuard.NotNull(weavingOptions?.Value);
        _logger = VKGuard.NotNull(logger);
    }

    public VKStageSchedule Schedule => VKPsychePipelineScheduler.Before.PsycheEcho;
    public bool IsActive => true;

    /// <summary>
    /// Resolves active session memories, prunes the history (oldest first) using dynamic budgets,
    /// and caches back to weaving context.
    /// </summary>
    public async Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken cancellationToken)
    {
        VKGuard.NotNull(context);

        var disabledTiers = context.Args<VKWeavingArgs>()?.DisabledTiers ?? _weavingOptions.DisabledTiers;
        if (disabledTiers is not null && disabledTiers.Contains(VKPromptTierType.Echo))
        {
            return VKResult.Success();
        }

        // 1. Fetch the updated history
        var historyResult = await _echoStore.GetHistoryAsync(context.Request.SessionId, cancellationToken).ConfigureAwait(false);
        if (historyResult.IsFailure)
        {
            return VKResult.Failure(historyResult.Errors);
        }
        var tierType = VKPromptTierType.Echo;
        var baseRenderOrder = context.Args<VKWeavingArgs>()?.TierRenderOrderOverrides?.IndexOf(tierType) is int idx && idx >= 0
            ? idx * PsycheConstants.Layout.TierCoordinateGap
            : PromptLayout.DefaultRenderOrders[tierType];

        var allEchoes = historyResult.Value;

        // Apply sliding window constraint (MaxWindowSize) if defined in request overrides or options
        var maxWindowSize = context.Args<VKEchoArgs>()?.MaxWindowSize ?? _echoOptions.MaxWindowSize;
        if (maxWindowSize.HasValue && maxWindowSize.Value > 0 && allEchoes.Count > maxWindowSize.Value)
        {
            allEchoes = [.. allEchoes.Skip(allEchoes.Count - maxWindowSize.Value)];
        }

        // 2. Filter System Messages if disabled
        if (!_echoOptions.IncludeSystemMessages)
        {
            allEchoes = [.. allEchoes.Where(e => e.Role != VKChatRole.System)];
        }

        // 3. Resolve Effective Token Budget
        int effectiveBudget = int.MaxValue;
        if (_echoOptions.MaxTokens.HasValue && _echoOptions.MaxTokens.Value > 0)
        {
            effectiveBudget = _echoOptions.MaxTokens.Value;
        }

        var totalLimit = context.Args<VKWeavingArgs>()?.TotalContextLimit ?? _weavingOptions.TotalContextLimit;
        int dynamicLimit = (int)(totalLimit * _echoOptions.TokenBudgetRatio);
        effectiveBudget = Math.Min(effectiveBudget, dynamicLimit);

        // 4. Trim dialogue history (from oldest to newest)
        var retained = new List<VKEchoTrace>();

        if (_echoOptions.PruneUnit == VKEchoPruneUnit.Turn)
        {
            // Prune by whole Turns (alternating user dialog steps)
            var turns = GroupIntoTurns([.. allEchoes]);
            int currentTokensSum = 0;
            int retainedTurnsCount = 0;

            foreach (var turn in turns)
            {
                int turnTokens = turn.Sum(e => _tokenCounter.CountTokens(e.Content));

                var maxTurns = context.Args<VKEchoArgs>()?.MaxTurns ?? _echoOptions.MaxTurns;
                if (maxTurns.HasValue && retainedTurnsCount >= maxTurns.Value)
                {
                    break;
                }

                if (currentTokensSum + turnTokens <= effectiveBudget)
                {
                    foreach (var item in turn)
                    {
                        retained.Insert(0, item); // Maintain oldest-first chronological order
                    }
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
            var list = allEchoes.ToList();

            for (int i = list.Count - 1; i >= 0; i--)
            {
                var item = list[i];
                int itemTokens = _tokenCounter.CountTokens(item.Content);

                if (currentTokensSum + itemTokens <= effectiveBudget)
                {
                    retained.Insert(0, item); // Prepend to preserve oldest-first
                    currentTokensSum += itemTokens;
                }
                else
                {
                    break; // Over budget: drop remaining oldest messages
                }
            }
        }
        // 5. Output pruned history -> map retained VKEchoTrace list to VKEchoCollectionMetadata
        for (int i = 0; i < retained.Count; i++)
        {
            context.AddFragment(new VKPromptFragment
            {
                TierType = tierType,
                RenderOrder = baseRenderOrder + i,
                Metadata = retained[i],
                Segment = new VKPromptSegment
                {
                    Role = retained[i].Role
                }
            });
        }

        _logger.EchoTrimmed(context.Request.SessionId, allEchoes.Count, retained.Count);

        return VKResult.Success();
    }

    /// <summary>
    /// Groups dialogue history into turn exchanges (from newest to oldest).
    /// </summary>
    private static List<List<VKEchoTrace>> GroupIntoTurns(IReadOnlyList<VKEchoTrace> echoes)
    {
        var turns = new List<List<VKEchoTrace>>();
        if (echoes.Count == 0)
        {
            return turns;
        }

        var currentTurn = new List<VKEchoTrace>();

        for (int i = echoes.Count - 1; i >= 0; i--)
        {
            var echo = echoes[i];
            currentTurn.Add(echo);

            // A turn is completed when we reach a User message going backward
            if (echo.Role == VKChatRole.User)
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
