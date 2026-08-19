using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VK.Blocks.AI.Psyche.Common.Internal;
using VK.Blocks.AI.Psyche.Echo.Diagnostics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Echo.Internal;

/// <summary>
/// Default implementation of the dialogue echo stage.
/// Retains and sliding-window trims short-term dialogue history.
/// Respects physical model limits dynamically via <see cref="IVKModelCatalog"/>.
/// Follows AP.01 (sealed class default) and CS.03.
/// </summary>
internal sealed class DefaultEchoExtractStage : IVKPsychePipelineStage
{
    private readonly IVKEchoStore _echoStore;
    private readonly IVKSessionStore _sessionStore;
    private readonly IVKTokenCounter _tokenCounter;
    private readonly IVKModelCatalog _modelCatalog;
    private readonly VKEchoOptions _echoOptions;
    private readonly VKWeavingOptions _weavingOptions;
    private readonly ILogger<DefaultEchoExtractStage> _logger;

    public DefaultEchoExtractStage(
        IVKEchoStore echoStore,
        IVKSessionStore sessionStore,
        IVKTokenCounter tokenCounter,
        IVKModelCatalog modelCatalog,
        VKEchoOptions echoOptions,
        VKWeavingOptions weavingOptions,
        ILogger<DefaultEchoExtractStage> logger)
    {
        _echoStore = VKGuard.NotNull(echoStore);
        _sessionStore = VKGuard.NotNull(sessionStore);
        _tokenCounter = VKGuard.NotNull(tokenCounter);
        _modelCatalog = VKGuard.NotNull(modelCatalog);
        _echoOptions = VKGuard.NotNull(echoOptions);
        _weavingOptions = VKGuard.NotNull(weavingOptions);
        _logger = VKGuard.NotNull(logger);
    }

    public VKPipelineSchedule Schedule => VKPsychePipelineScheduler.Before.PsycheEcho;
    public bool IsActive => _echoOptions.Enabled;

    public async Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(context);

        var stopwatch = Stopwatch.StartNew();
        try
        {
            var disabledTiers = context.Args<VKWeavingArgs>()?.DisabledTiers ?? _weavingOptions.DisabledTiers;
            if (disabledTiers is not null && disabledTiers.Contains(VKPromptTierType.Echo))
            {
                return VKResult.Success();
            }

            if (context.Request.SessionId.IsEmpty)
            {
                return VKResult.Success();
            }

            // 1. Fetch history (supports Continuous multi-level parent ancestry tracing)
            var allEchoes = new List<VKEchoTrace>();
            var currentSessionId = (VKSessionId?)context.Request.SessionId;
            var mode = context.State<VKSessionThread>()?.Mode ?? VKSessionMode.Isolated;

            var visitedSessions = new HashSet<VKSessionId>();

            while (currentSessionId.HasValue && visitedSessions.Add(currentSessionId.Value))
            {
                var historyResult = await _echoStore.GetHistoryAsync(currentSessionId.Value, cancellationToken).ConfigureAwait(false);
                if (historyResult.IsSuccess && historyResult.Value.Count > 0)
                {
                    // Prepend parent echoes before child echoes
                    allEchoes.InsertRange(0, historyResult.Value);
                }

                // Only trace parent dynamically if mode is Continuous
                if (mode == VKSessionMode.Continuous)
                {
                    var cachedSession = context.State<VKSessionThread>();
                    if (cachedSession is not null && cachedSession.Id == currentSessionId.Value)
                    {
                        currentSessionId = cachedSession.ParentSessionId;
                    }
                    else
                    {
                        var sessionResult = await _sessionStore.GetSessionAsync(currentSessionId.Value, cancellationToken).ConfigureAwait(false);
                        currentSessionId = sessionResult.IsSuccess ? sessionResult.Value?.ParentSessionId : null;
                    }
                }
                else
                {
                    break;
                }
            }

            if (allEchoes.Count == 0)
            {
                return VKResult.Success();
            }

            // 1. Apply sliding window constraint (MaxWindowSize) if defined in request overrides or options
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

            if (allEchoes.Count == 0)
            {
                return VKResult.Success();
            }

            // 3. Resolve Effective Token Budget dynamically based on IVKModelCatalog & MaxContextBudget
            var modelId = context.Args<VKChatArgs>()?.ModelId ?? string.Empty;
            var modelMetadata = _modelCatalog.GetModelMetadata(modelId);

            var configuredBudget = context.Args<VKWeavingArgs>()?.MaxContextBudget ?? _weavingOptions.MaxContextBudget;
            var totalLimit = configuredBudget.HasValue
                ? Math.Min(configuredBudget.Value, modelMetadata.ContextWindowSize)
                : modelMetadata.ContextWindowSize;

            int effectiveBudget = int.MaxValue;
            if (_echoOptions.MaxTokens.HasValue && _echoOptions.MaxTokens.Value > 0)
            {
                effectiveBudget = _echoOptions.MaxTokens.Value;
            }

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
                    int turnTokens = turn.Sum(GetEchoTokens);

                    var maxTurns = context.Args<VKEchoArgs>()?.MaxTurns ?? _echoOptions.MaxTurns;
                    if (maxTurns.HasValue && retainedTurnsCount >= maxTurns.Value)
                    {
                        break;
                    }

                    if (currentTokensSum + turnTokens <= effectiveBudget)
                    {
                        retained.InsertRange(0, turn); // Maintain oldest-first chronological order and intra-turn order
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

                for (int i = allEchoes.Count - 1; i >= 0; i--)
                {
                    var item = allEchoes[i];
                    int itemTokens = GetEchoTokens(item);

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

            var tierType = VKPromptTierType.Echo;
            var baseRenderOrder = context.Args<VKWeavingArgs>()?.TierRenderOrderOverrides?.IndexOf(tierType) is int idx && idx >= 0
                ? idx * PsycheConstants.Layout.TierCoordinateGap
                : PromptLayout.DefaultRenderOrders[tierType];

            for (var i = 0; i < retained.Count; i++)
            {
                var echo = retained[i];
                context.AddFragment(new VKPromptFragment()
                {
                    TierType = tierType,
                    RenderOrder = baseRenderOrder + i,
                    Metadata = echo,
                    Segment = new VKPromptSegment()
                    {
                        Content = echo.Content,
                        Role = echo.Role,
                        RelativeDepth = VKPromptRelativeDepth.AfterEcho
                    }
                });
            }

            _logger.EchoTrimmed(context.Request.SessionId, allEchoes.Count, retained.Count);

            return VKResult.Success();
        }
        finally
        {
            stopwatch.Stop();
            context.ResponseBuilder.ProfilingMetrics[VKPsycheProfilingKeys.EchoExtractStage] = stopwatch.Elapsed.TotalMilliseconds;
        }
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

    private int GetEchoTokens(VKEchoTrace echo)
    {
        return echo.TokenCount > 0 ? echo.TokenCount : _tokenCounter.CountTokens(echo.Content);
    }
}
