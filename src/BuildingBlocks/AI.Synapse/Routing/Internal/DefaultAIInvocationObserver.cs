using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.AI.Synapse.Common.Diagnostics.Internal;
using VK.Blocks.Core;
using VK.Blocks.Resilience;

namespace VK.Blocks.AI.Synapse.Routing.Internal;

/// <summary>
/// Default invocation observer that updates circuit health, affinity pins, cost ledger, and token budget state.
/// </summary>
// [AP.01] [ARCH-02] [CS.03] [CS.06]
internal sealed class DefaultAIInvocationObserver : IVKAIInvocationObserver
{
    private readonly IVKAIProviderPool? _providerPool;
    private readonly IVKAIProviderTracker? _providerTracker;
    private readonly IVKCircuitBreaker? _circuitBreaker;
    private readonly IVKAICostCalculator? _costCalculator;
    private readonly IVKAICostLedger? _costLedger;
    private readonly IVKAITokenBudgetManager? _tokenBudgetManager;
    private readonly IVKAICumulativeQuotaStore? _cumulativeQuotaStore;
    private readonly IVKAIAffinityPinStore? _pinStore;
    private readonly IVKAIFailurePolicy? _failurePolicy;
    private readonly VKQuotaOptions? _quotaOptions;
    private readonly TimeProvider _timeProvider;

    public DefaultAIInvocationObserver(
        IVKAIProviderPool? providerPool = null,
        IVKAIProviderTracker? providerTracker = null,
        IVKCircuitBreaker? circuitBreaker = null,
        IVKAICostCalculator? costCalculator = null,
        IVKAICostLedger? costLedger = null,
        IVKAITokenBudgetManager? tokenBudgetManager = null,
        IVKAICumulativeQuotaStore? cumulativeQuotaStore = null,
        IVKAIAffinityPinStore? pinStore = null,
        IVKAIFailurePolicy? failurePolicy = null,
        VKQuotaOptions? quotaOptions = null,
        TimeProvider? timeProvider = null)
    {
        _providerPool = providerPool;
        _providerTracker = providerTracker;
        _circuitBreaker = circuitBreaker;
        _costCalculator = costCalculator;
        _costLedger = costLedger;
        _tokenBudgetManager = tokenBudgetManager;
        _cumulativeQuotaStore = cumulativeQuotaStore;
        _pinStore = pinStore;
        _failurePolicy = failurePolicy;
        _quotaOptions = quotaOptions;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public bool IsNoOp => false;

    public async ValueTask ReportAsync(
        VKAIInvocationOutcome outcome,
        CancellationToken cancellationToken = default)
    {
        // [AP.01] boundary guard
        VKGuard.NotNull(outcome);

        var affinityKey = !string.IsNullOrWhiteSpace(outcome.AffinityKey)
            ? outcome.AffinityKey
            : outcome.CorrelationTags?.RunId ?? outcome.CorrelationTags?.SessionId;
        var reservationId = !string.IsNullOrWhiteSpace(outcome.ReservationId)
            ? outcome.ReservationId
            : outcome.InvocationId;
        var inputTokens = outcome.InputTokens ?? 0;
        var outputTokens = outcome.OutputTokens ?? 0;
        var cachedTokens = outcome.CachedTokens ?? 0;
        var reasoningTokens = outcome.ReasoningTokens ?? 0;
        var totalTokens = inputTokens + outputTokens;
        var duration = outcome.Duration ?? TimeSpan.Zero;
        CancellationToken effectiveToken = !outcome.IsSuccess && cancellationToken.IsCancellationRequested
            ? CancellationToken.None
            : cancellationToken;

        try
        {
            VKAIFailureDirective? directive = _failurePolicy?.EvaluateOutcome(outcome);
            if (directive is not null)
            {
                ApplyHealthDirective(outcome, directive.HealthAction);
            }

            VKAIConnection connection = await ResolveConnectionAsync(outcome.ConnectionId, effectiveToken).ConfigureAwait(false);
            var provider = (connection.Provider ?? VKAIProviderType.OpenAI).ToString();
            var modelId = connection.ModelId ?? VKAIModelIds.OpenAI.Gpt6Luna;

            // SYN-10: Emit only low-cardinality metric tags (never ScopeKey or AffinityKey).
            AISynapseDiagnostics.RecordRequest(provider, modelId, outcome.IsSuccess, duration.TotalMilliseconds);

            if (outcome.IsSuccess)
            {
                if (_pinStore is not null && !string.IsNullOrWhiteSpace(affinityKey))
                {
                    await _pinStore.PinIfUnsetAsync(affinityKey, outcome.ConnectionId, cancellationToken: cancellationToken).ConfigureAwait(false);
                }

                _providerTracker?.RecordMetrics(connection, totalTokens, duration);

                var costResult = _costCalculator is not null
                    ? _costCalculator.CalculateCost(
                        provider,
                        modelId,
                        inputTokens,
                        outputTokens,
                        cachedTokens,
                        reasoningTokens)
                    : VKAICostResult.Unpriced;

                AISynapseDiagnostics.RecordTokensAndCost(provider, modelId, totalTokens, (double)costResult.Amount);

                if (outcome.ScopeKey is { } scopeKey)
                {
                    if (_costLedger is not null)
                    {
                        await _costLedger.RecordAsync(new VKAICostLedgerEntry
                        {
                            InvocationId = outcome.InvocationId,
                            AttemptNo = outcome.AttemptNo,
                            ScopeKey = scopeKey,
                            Tags = outcome.CorrelationTags ?? new VKAICorrelationTags(),
                            Provider = provider,
                            ModelId = modelId,
                            Cost = costResult,
                            Usage = new VKAIUsageMetrics
                            {
                                PromptTokens = inputTokens,
                                CompletionTokens = outputTokens,
                                CachedTokens = cachedTokens,
                                ReasoningTokens = reasoningTokens,
                                Duration = duration,
                                EstimatedCost = costResult.Amount
                            },
                            RecordedAt = _timeProvider.GetUtcNow()
                        }, cancellationToken).ConfigureAwait(false);
                    }

                    if (_cumulativeQuotaStore is not null)
                    {
                        await _cumulativeQuotaStore.RecordUsageAsync(
                            scopeKey,
                            totalTokens,
                            costResult.Amount,
                            cancellationToken).ConfigureAwait(false);
                    }
                }
            }
            else
            {
                AISynapseDiagnostics.RecordProviderFailure(provider, modelId, outcome.FailureKind.ToString());

                if (_pinStore is not null &&
                    directive is { UnpinAffinity: true } &&
                    !string.IsNullOrWhiteSpace(affinityKey))
                {
                    await _pinStore.UnpinAsync(affinityKey, outcome.ConnectionId, effectiveToken).ConfigureAwait(false);
                }
            }
        }
        catch (Exception)
        {
            // SYN-01: Observer failures must not propagate into the invocation pipeline.
        }

        // SYN-01 & SYN-07: Isolate token budget settlement/release from cost, quota, and health errors.
        if (outcome.ScopeKey is { } tokenScopeKey && _tokenBudgetManager is not null)
        {
            try
            {
                if (outcome.IsSuccess)
                {
                    await _tokenBudgetManager.SettleTokensAsync(
                        tokenScopeKey,
                        reservationId,
                        totalTokens,
                        cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    await _tokenBudgetManager.ReleaseTokensAsync(
                        tokenScopeKey,
                        reservationId,
                        effectiveToken).ConfigureAwait(false);
                }
            }
            catch (Exception)
            {
                // SYN-01: Observer failures must not propagate into the invocation pipeline.
            }
        }
    }

    private async ValueTask<VKAIConnection> ResolveConnectionAsync(string connectionId, CancellationToken cancellationToken)
    {
        VKAIConnection? connection = null;
        if (_providerPool is not null)
        {
            var poolResult = await _providerPool.GetAvailablePoolAsync(cancellationToken).ConfigureAwait(false);
            if (poolResult.IsSuccess)
            {
                connection = poolResult.Value.FirstOrDefault(c =>
                    string.Equals(c.Id, connectionId, StringComparison.OrdinalIgnoreCase));
            }
        }

        return connection ?? new VKAIConnection
        {
            Id = connectionId,
            Name = connectionId
        };
    }

    private void ApplyHealthDirective(VKAIInvocationOutcome outcome, VKAIHealthAction healthAction)
    {
        if (_circuitBreaker is null)
        {
            return;
        }

        TimeSpan defaultCooldown = _quotaOptions?.DefaultCooldownDuration ?? TimeSpan.FromSeconds(30);
        int defaultThreshold = _quotaOptions?.DefaultCircuitBreakerThreshold ?? 3;

        switch (healthAction)
        {
            case VKAIHealthAction.RecordSuccess:
                _circuitBreaker.RecordOutcome(
                    outcome.ConnectionId,
                    isSuccess: true,
                    elapsed: outcome.Duration,
                    minimumThroughput: defaultThreshold);
                break;

            case VKAIHealthAction.RecordCooldown:
                var cooldown = outcome.RetryAfter is { } ra && ra > TimeSpan.Zero
                    ? ra
                    : defaultCooldown;
                _circuitBreaker.RecordCooldown(
                    outcome.ConnectionId,
                    cooldown,
                    reason: $"HTTP {outcome.HttpStatusCode ?? 429} RateLimited");
                break;

            case VKAIHealthAction.Isolate:
                _circuitBreaker.Isolate(
                    outcome.ConnectionId,
                    reason: $"HTTP {outcome.HttpStatusCode ?? 401} AuthFailed");
                break;

            case VKAIHealthAction.RecordFailure:
                _circuitBreaker.RecordOutcome(
                    outcome.ConnectionId,
                    isSuccess: false,
                    elapsed: outcome.Duration,
                    failureReason: outcome.FailureKind.ToString(),
                    cooldownDuration: defaultCooldown,
                    minimumThroughput: defaultThreshold,
                    failureRatio: 0.5);
                break;

            case VKAIHealthAction.None:
            default:
                if (!outcome.IsSuccess)
                {
                    _circuitBreaker.ReleaseHalfOpenProbe(outcome.ConnectionId);
                }
                break;
        }
    }
}
