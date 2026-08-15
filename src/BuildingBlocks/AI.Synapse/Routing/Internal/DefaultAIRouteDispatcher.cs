using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using VK.Blocks.AI;
using VK.Blocks.AI.Synapse.Diagnostics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Synapse.Routing.Internal;

// [AP.01] sealed
internal sealed class DefaultAIRouteDispatcher : IVKAIRouteDispatcher
{
    private readonly IVKAIRouter _router;
    private readonly IVKAIProviderTracker _tracker;
    private readonly IVKAIProviderPool _providerPool;
    private readonly IVKIdentityContext _identityContext;
    private readonly VKRoutingOptions _routingOptions;
    private readonly IVKAICostCalculator? _costCalculator;
    private readonly IVKAITokenBudgetManager? _tokenBudgetManager;
    private readonly IVKAIEngineAccessor? _engineAccessor;
    private readonly ILogger<DefaultAIRouteDispatcher> _logger;

    public DefaultAIRouteDispatcher(
        IVKAIRouter router,
        IVKAIProviderTracker tracker,
        IVKAIProviderPool providerPool,
        IVKIdentityContext identityContext,
        VKRoutingOptions routingOptions,
        IVKAICostCalculator? costCalculator = null,
        IVKAITokenBudgetManager? tokenBudgetManager = null,
        IVKAIEngineAccessor? engineAccessor = null,
        ILogger<DefaultAIRouteDispatcher>? logger = null)
    {
        _router = VKGuard.NotNull(router);
        _tracker = VKGuard.NotNull(tracker);
        _providerPool = VKGuard.NotNull(providerPool);
        _identityContext = VKGuard.NotNull(identityContext);
        _routingOptions = VKGuard.NotNull(routingOptions);
        _costCalculator = costCalculator;
        _tokenBudgetManager = tokenBudgetManager;
        _engineAccessor = engineAccessor;
        _logger = logger ?? NullLogger<DefaultAIRouteDispatcher>.Instance;
    }

    public async Task<VKResult<VKAIConnection>> SelectCandidateAsync(
        VKAIRouteArgs? args = null,
        CancellationToken cancellationToken = default)
    {
        args ??= new VKAIRouteArgs();

        var poolResult = await _providerPool.GetAvailablePoolAsync(cancellationToken).ConfigureAwait(false);
        if (poolResult.IsFailure || poolResult.Value.Count == 0)
        {
            return VKResult.Failure<VKAIConnection>(VKAISynapseErrors.NoAvailableProvider);
        }

        var candidateResult = await _router.ResolveCandidatesAsync(args, poolResult.Value, cancellationToken).ConfigureAwait(false);
        if (candidateResult.IsFailure || candidateResult.Value.Count == 0)
        {
            return VKResult.Failure<VKAIConnection>(VKAISynapseErrors.NoAvailableProvider);
        }

        return VKResult.Success(candidateResult.Value[0]);
    }

    public async Task<VKResult<TResponse>> ExecuteWithFallbackAsync<TResponse>(
        VKAIRouteArgs? args,
        Func<VKAIConnection, CancellationToken, Task<VKResult<TResponse>>> operation,
        CancellationToken cancellationToken = default)
    {
        args ??= new VKAIRouteArgs();
        VKGuard.NotNull(operation);

        var poolResult = await _providerPool.GetAvailablePoolAsync(cancellationToken).ConfigureAwait(false);
        if (poolResult.IsFailure || poolResult.Value.Count == 0)
        {
            return VKResult.Failure<TResponse>(VKAISynapseErrors.NoAvailableProvider);
        }

        var candidatesResult = await _router.ResolveCandidatesAsync(args, poolResult.Value, cancellationToken).ConfigureAwait(false);
        if (candidatesResult.IsFailure || candidatesResult.Value.Count == 0)
        {
            return VKResult.Failure<TResponse>(VKAISynapseErrors.NoAvailableProvider);
        }

        var candidates = candidatesResult.Value;
        int maxAttempts = Math.Min(candidates.Count, _routingOptions.MaxFallbackAttempts);
        VKAIProviderType? initialProviderType = candidates.Count > 0 ? candidates[0].Provider : null;

        using var overallCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        overallCts.CancelAfter(_routingOptions.OverallTimeout);
        var overallToken = overallCts.Token;

        for (int i = 0; i < maxAttempts; i++)
        {
            var connection = candidates[i];

            // If cross-provider fallback is disabled, skip candidate if provider type changes
            if (!_routingOptions.EnableCrossProviderFallback && i > 0 && connection.Provider != initialProviderType)
            {
                continue;
            }

            string providerName = connection.Provider?.ToString() ?? "Unknown";
            string modelId = connection.ModelId ?? "Unknown";

            _tracker.RecordRequest(connection);
            _logger.RequestRouted(providerName, modelId, args.OperationKey);

            using var attemptCts = CancellationTokenSource.CreateLinkedTokenSource(overallToken);
            attemptCts.CancelAfter(_routingOptions.RequestTimeout);
            var attemptToken = attemptCts.Token;

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                var result = await operation(connection, attemptToken).ConfigureAwait(false);
                stopwatch.Stop();

                if (result.IsSuccess)
                {
                    _tracker.MarkSuccess(connection);
                    AISynapseDiagnostics.RecordRequest(providerName, modelId, true, stopwatch.Elapsed.TotalMilliseconds);

                    // Record tokens and cost if response provides usage
                    await TryRecordUsageAndCostAsync(result.Value, providerName, modelId, _identityContext.TenantId.ToString(), cancellationToken).ConfigureAwait(false);

                    return result;
                }

                _tracker.MarkFailure(connection, new InvalidOperationException(result.FirstError.Description));
                AISynapseDiagnostics.RecordRequest(providerName, modelId, false, stopwatch.Elapsed.TotalMilliseconds);
                _logger.ProviderFailedFallback(providerName, modelId, result.FirstError.Description);
            }
            catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
            {
                stopwatch.Stop();
                _tracker.MarkFailure(connection, ex);
                AISynapseDiagnostics.RecordRequest(providerName, modelId, false, stopwatch.Elapsed.TotalMilliseconds);
                _logger.ProviderFailedFallback(providerName, modelId, ex.Message);
            }
        }

        return VKResult.Failure<TResponse>(VKAISynapseErrors.AllProvidersFailed);
    }

    public async Task<VKResult<VKChatResponse>> ExecuteChatWithFallbackAsync(
        IEnumerable<VKChatMessage> messages,
        VKAIRouteArgs? args = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(messages);
        args ??= new VKAIRouteArgs();

        return await ExecuteWithFallbackAsync(args, async (connection, attemptToken) =>
        {
            if (!connection.Provider.HasValue)
            {
                return VKResult.Failure<VKChatResponse>(VKAISynapseErrors.InvalidConfiguration);
            }

            var engine = _engineAccessor?.GetChatEngine(connection.Provider.Value);
            if (engine is null)
            {
                return VKResult.Failure<VKChatResponse>(
                    new VKError("AISynapse.EngineNotRegistered", $"No Keyed IVKChatEngine registered for provider '{connection.Provider.Value}'."));
            }

            return await engine.SendAsync(messages, null, attemptToken).ConfigureAwait(false);
        }, cancellationToken).ConfigureAwait(false);
    }

    private async Task TryRecordUsageAndCostAsync<TResponse>(
        TResponse? response,
        string providerName,
        string modelId,
        string? tenantId,
        CancellationToken cancellationToken)
    {
        if (response is null)
            return;

        long promptTokens = 0;
        long completionTokens = 0;

        if (response is VKChatResponse chatResponse && chatResponse.Usage != null)
        {
            promptTokens = chatResponse.Usage.InputTokens;
            completionTokens = chatResponse.Usage.OutputTokens;
        }
        else if (response is VKAITokenUsage tokenUsage)
        {
            promptTokens = tokenUsage.InputTokens;
            completionTokens = tokenUsage.OutputTokens;
        }

        long totalTokens = promptTokens + completionTokens;
        if (totalTokens > 0)
        {
            double cost = _costCalculator?.CalculateCost(providerName, modelId, promptTokens, completionTokens) ?? 0.0;
            AISynapseDiagnostics.RecordTokensAndCost(providerName, modelId, totalTokens, cost);

            if (!string.IsNullOrEmpty(tenantId) && _tokenBudgetManager != null)
            {
                await _tokenBudgetManager.RecordUsageAsync(tenantId, (int)Math.Min(totalTokens, int.MaxValue), cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
