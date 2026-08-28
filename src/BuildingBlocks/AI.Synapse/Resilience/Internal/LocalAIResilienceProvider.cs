using System;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.AI.Synapse.Diagnostics.Internal;
using VK.Blocks.Core;
using VK.Blocks.Resilience;

namespace VK.Blocks.AI.Synapse.Resilience.Internal;

// [AP.01] sealed
internal sealed class LocalAIResilienceProvider : IVKAIResilienceProvider
{
    private readonly IVKCircuitBreaker _circuitBreaker;
    private readonly VKAIResilienceOptions _options;
    private readonly TimeProvider _timeProvider;

    public LocalAIResilienceProvider(
        IVKCircuitBreaker circuitBreaker,
        VKAIResilienceOptions? options = null,
        TimeProvider? timeProvider = null)
    {
        _circuitBreaker = VKGuard.NotNull(circuitBreaker);
        _options = options ?? new VKAIResilienceOptions();
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public async Task<VKResult<T>> ExecuteWithProviderFallbackAsync<T>(
        string primaryProviderName,
        string fallbackProviderName,
        Func<string, CancellationToken, Task<VKResult<T>>> executeWithProvider,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(primaryProviderName);
        VKGuard.NotNullOrWhiteSpace(fallbackProviderName);
        VKGuard.NotNull(executeWithProvider);
        cancellationToken.ThrowIfCancellationRequested();

        var primaryKey = $"ai:provider:{primaryProviderName.ToLowerInvariant()}";
        var fallbackKey = $"ai:provider:{fallbackProviderName.ToLowerInvariant()}";

        // Try primary if circuit is allowed
        if (_circuitBreaker.IsAllowed(primaryKey))
        {
            try
            {
                var primaryResult = await executeWithProvider(primaryProviderName, cancellationToken).ConfigureAwait(false);
                if (primaryResult.IsSuccess)
                {
                    _circuitBreaker.RecordSuccess(primaryKey);
                    AISynapseDiagnostics.RecordRequest(primaryProviderName, "primary", true, 0);
                    return primaryResult;
                }

                _circuitBreaker.RecordFailure(
                    primaryKey,
                    new InvalidOperationException(primaryResult.FirstError.Description),
                    _options.ProviderCooldown);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _circuitBreaker.RecordFailure(primaryKey, ex, _options.ProviderCooldown);
            }
        }

        // Failover to secondary provider
        AISynapseDiagnostics.RecordProviderFailure(primaryProviderName, "fallback", "Primary provider circuit open or failed, attempting failover");

        try
        {
            var fallbackResult = await executeWithProvider(fallbackProviderName, cancellationToken).ConfigureAwait(false);
            if (fallbackResult.IsSuccess)
            {
                _circuitBreaker.RecordSuccess(fallbackKey);
            }
            else
            {
                _circuitBreaker.RecordFailure(
                    fallbackKey,
                    new InvalidOperationException(fallbackResult.FirstError.Description),
                    _options.ProviderCooldown);
            }

            return fallbackResult;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _circuitBreaker.RecordFailure(fallbackKey, ex, _options.ProviderCooldown);
            return VKResult.Failure<T>(VKAISynapseErrors.CreateFallbackFailed(ex.Message));
        }
    }

    public async Task<VKResult<T>> ExecuteWithModelFallbackAsync<T>(
        string primaryModelId,
        string fallbackModelId,
        Func<string, CancellationToken, Task<VKResult<T>>> executeWithModel,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(primaryModelId);
        VKGuard.NotNullOrWhiteSpace(fallbackModelId);
        VKGuard.NotNull(executeWithModel);
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var primaryResult = await executeWithModel(primaryModelId, cancellationToken).ConfigureAwait(false);
            if (primaryResult.IsSuccess)
            {
                AISynapseDiagnostics.RecordRequest("model", primaryModelId, true, 0);
                return primaryResult;
            }

            // If primary model failed, downgrade to fallback model
            AISynapseDiagnostics.RecordProviderFailure("model", primaryModelId, "Primary model execution failed, downgrading");
            return await executeWithModel(fallbackModelId, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            AISynapseDiagnostics.RecordProviderFailure("model", primaryModelId, "Primary model exception, downgrading");
            try
            {
                return await executeWithModel(fallbackModelId, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception fallbackEx)
            {
                return VKResult.Failure<T>(VKAISynapseErrors.CreateFallbackFailed(fallbackEx.Message));
            }
        }
    }

    public async Task<VKResult<T>> ExecuteWithRateLimitRetryAsync<T>(
        Func<CancellationToken, Task<VKResult<T>>> action,
        int maxRetries = 3,
        TimeSpan? defaultRetryAfter = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(action);
        cancellationToken.ThrowIfCancellationRequested();

        var baseRetryAfter = defaultRetryAfter ?? _options.DefaultRateLimitRetryDelay;
        VKResult<T>? lastResult = null;

        for (int attempt = 0; attempt <= maxRetries; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var result = await action(cancellationToken).ConfigureAwait(false);
                if (result.IsSuccess)
                {
                    AISynapseDiagnostics.RecordRequest("ratelimit", "retry", true, 0);
                    return result;
                }

                lastResult = result;
                var errorCode = result.FirstError.Code ?? string.Empty;

                bool isRateLimit = errorCode.Contains("429", StringComparison.OrdinalIgnoreCase) ||
                                   errorCode.Contains("RateLimit", StringComparison.OrdinalIgnoreCase) ||
                                   errorCode.Contains("QuotaExceeded", StringComparison.OrdinalIgnoreCase);

                if (!isRateLimit || attempt == maxRetries)
                {
                    break;
                }

                // Exponential backoff for RateLimit retry
                var delay = TimeSpan.FromMilliseconds(baseRetryAfter.TotalMilliseconds * Math.Pow(1.5, attempt));
                await Task.Delay(delay, _timeProvider, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                if (attempt == maxRetries)
                {
                    return VKResult.Failure<T>(VKAISynapseErrors.CreateRetryExhausted(ex.Message));
                }

                var delay = TimeSpan.FromMilliseconds(baseRetryAfter.TotalMilliseconds * Math.Pow(1.5, attempt));
                await Task.Delay(delay, _timeProvider, cancellationToken).ConfigureAwait(false);
            }
        }

        return lastResult ?? VKResult.Failure<T>(VKAISynapseErrors.CreateRetryExhausted("RateLimit retries exhausted."));
    }
}
