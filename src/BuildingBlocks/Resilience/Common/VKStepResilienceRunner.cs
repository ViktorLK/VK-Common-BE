using System;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.Resilience;

/// <summary>
/// Industrial runner that encapsulates Timeout, Circuit Breaker, Retry, and graceful fallback
/// into a single seamless, zero-allocation pipeline execution.
/// Follows [AP.01], [CS.01], [CS.03], [OR.03].
/// </summary>
public static class VKStepResilienceRunner
{
    public static async Task<VKResult<T>> ExecuteWithResilienceAsync<T>(
        this VKStepResiliencePolicy policy,
        Func<CancellationToken, Task<VKResult<T>>> action,
        IVKRetryExecutor? retryExecutor,
        IVKTimeoutExecutor? timeoutExecutor,
        IVKCircuitBreaker? circuitBreaker,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        VKGuard.NotNull(policy);
        VKGuard.NotNull(action);
        cancellationToken.ThrowIfCancellationRequested();

        var cbKey = policy.CircuitBreaker?.CircuitBreakerKey;

        // 1. Circuit Breaker pre-flight check
        if (!string.IsNullOrEmpty(cbKey) && circuitBreaker is not null && !circuitBreaker.IsAllowed(cbKey))
        {
            return VKResult.Failure<T>(VKResilienceErrors.CircuitBreakerOpen);
        }

        var retryPolicy = policy.Retry ?? VKStepRetryPolicy.Default;
        var timeoutPolicy = policy.Timeout;

        // Local wrapper to apply timeout on a single attempt
        async Task<VKResult<T>> ExecuteSingleAttemptAsync(CancellationToken ct)
        {
            if (timeoutPolicy is not null && timeoutExecutor is not null)
            {
                var timeoutResult = await timeoutExecutor.ExecuteWithTimeoutAsync(
                    async innerCt => await action(innerCt).ConfigureAwait(false),
                    timeout: timeoutPolicy.Timeout,
                    cancellationToken: ct).ConfigureAwait(false);

                return timeoutResult.IsSuccess
                    ? timeoutResult.Value
                    : VKResult.Failure<T>(timeoutResult.FirstError);
            }

            return await action(ct).ConfigureAwait(false);
        }

        VKResult<T> finalResult;

        // 2. Execute via IVKRetryExecutor if registered
        if (retryExecutor is not null)
        {
            finalResult = await retryExecutor.ExecuteWithRetryAsync(
                ExecuteSingleAttemptAsync,
                maxRetries: retryPolicy.MaxRetries,
                initialDelay: TimeSpan.FromMilliseconds(retryPolicy.InitialDelayMs),
                shouldRetry: error => retryPolicy.IsTransient(error),
                cancellationToken: cancellationToken).ConfigureAwait(false);
        }
        else
        {
            // 3. Fallback in-memory retry loop (when Resilience block is not registered)
            var maxAttempts = Math.Max(0, retryPolicy.MaxRetries) + 1;
            finalResult = VKResult.Failure<T>(VKResilienceErrors.CreateRetryExhausted("Initial failure"));

            for (var attempt = 1; attempt <= maxAttempts; attempt++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    finalResult = await ExecuteSingleAttemptAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    finalResult = VKResult.Failure<T>(VKResilienceErrors.CreateRetryExhausted(ex.Message));
                }

                if (finalResult.IsSuccess)
                {
                    break;
                }

                var error = finalResult.FirstError;
                if (!retryPolicy.IsTransient(error) || attempt >= maxAttempts)
                {
                    break;
                }

                var delay = retryPolicy.CalculateDelay(attempt);
                await Task.Delay(delay, timeProvider, cancellationToken).ConfigureAwait(false);
            }
        }

        // 4. Circuit Breaker post-execution recording
        if (!string.IsNullOrEmpty(cbKey) && circuitBreaker is not null)
        {
            if (finalResult.IsSuccess)
            {
                circuitBreaker.RecordSuccess(cbKey);
            }
            else
            {
                circuitBreaker.RecordFailure(cbKey, new InvalidOperationException(finalResult.FirstError.Description));
            }
        }

        return finalResult;
    }
}
