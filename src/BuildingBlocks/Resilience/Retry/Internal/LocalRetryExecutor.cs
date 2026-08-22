using System;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;
using VK.Blocks.Resilience.Diagnostics.Internal;

namespace VK.Blocks.Resilience.Retry.Internal;

// [AP.01] sealed
internal sealed class LocalRetryExecutor : IVKRetryExecutor
{
    private readonly VKRetryOptions _options;
    private readonly TimeProvider _timeProvider;

    public LocalRetryExecutor(VKRetryOptions options, TimeProvider? timeProvider = null)
    {
        _options = VKGuard.NotNull(options);
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    /// <inheritdoc />
    public async Task<VKResult<T>> ExecuteWithRetryAsync<T>(
        Func<CancellationToken, Task<VKResult<T>>> action,
        int? maxRetries = null,
        TimeSpan? initialDelay = null,
        Func<VKError, bool>? shouldRetry = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(action);

        int attemptsLimit = maxRetries ?? _options.MaxRetries;
        var currentDelay = initialDelay ?? _options.InitialDelay;
        VKResult<T>? lastResult = null;

        for (int attempt = 0; attempt <= attemptsLimit; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var result = await action(cancellationToken).ConfigureAwait(false);
                if (result.IsSuccess)
                {
                    ResilienceDiagnostics.RecordStrategyExecution("retry", true);
                    return result;
                }

                lastResult = result;

                if (attempt == attemptsLimit || (shouldRetry != null && !shouldRetry(result.FirstError)))
                {
                    break;
                }

                var delayToWait = CalculateDelay(currentDelay, attempt);
                await Task.Delay(delayToWait, _timeProvider, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                if (attempt == attemptsLimit)
                {
                    ResilienceDiagnostics.RecordStrategyExecution("retry", false);
                    return VKResult.Failure<T>(VKResilienceErrors.CreateRetryExhausted(ex.Message));
                }

                var delayToWait = CalculateDelay(currentDelay, attempt);
                await Task.Delay(delayToWait, _timeProvider, cancellationToken).ConfigureAwait(false);
            }
        }

        ResilienceDiagnostics.RecordStrategyExecution("retry", false);
        return lastResult ?? VKResult.Failure<T>(VKResilienceErrors.CreateRetryExhausted("Max retries reached."));
    }

    /// <inheritdoc />
    public async Task<VKResult> ExecuteWithRetryAsync(
        Func<CancellationToken, Task<VKResult>> action,
        int? maxRetries = null,
        TimeSpan? initialDelay = null,
        Func<VKError, bool>? shouldRetry = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(action);

        int attemptsLimit = maxRetries ?? _options.MaxRetries;
        var currentDelay = initialDelay ?? _options.InitialDelay;
        VKResult? lastResult = null;

        for (int attempt = 0; attempt <= attemptsLimit; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var result = await action(cancellationToken).ConfigureAwait(false);
                if (result.IsSuccess)
                {
                    ResilienceDiagnostics.RecordStrategyExecution("retry", true);
                    return result;
                }

                lastResult = result;

                if (attempt == attemptsLimit || (shouldRetry != null && !shouldRetry(result.FirstError)))
                {
                    break;
                }

                var delayToWait = CalculateDelay(currentDelay, attempt);
                await Task.Delay(delayToWait, _timeProvider, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                if (attempt == attemptsLimit)
                {
                    ResilienceDiagnostics.RecordStrategyExecution("retry", false);
                    return VKResult.Failure(VKResilienceErrors.CreateRetryExhausted(ex.Message));
                }

                var delayToWait = CalculateDelay(currentDelay, attempt);
                await Task.Delay(delayToWait, _timeProvider, cancellationToken).ConfigureAwait(false);
            }
        }

        ResilienceDiagnostics.RecordStrategyExecution("retry", false);
        return lastResult ?? VKResult.Failure(VKResilienceErrors.CreateRetryExhausted("Max retries reached."));
    }

    /// <inheritdoc />
    public async Task<VKResult<T>> ExecuteWithRetryAsync<T>(
        Func<CancellationToken, Task<T>> action,
        int? maxRetries = null,
        TimeSpan? initialDelay = null,
        Func<Exception, bool>? shouldRetry = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(action);

        int attemptsLimit = maxRetries ?? _options.MaxRetries;
        var currentDelay = initialDelay ?? _options.InitialDelay;
        Exception? lastException = null;

        for (int attempt = 0; attempt <= attemptsLimit; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var result = await action(cancellationToken).ConfigureAwait(false);
                ResilienceDiagnostics.RecordStrategyExecution("retry", true);
                return VKResult.Success(result);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                lastException = ex;

                if (attempt == attemptsLimit || (shouldRetry != null && !shouldRetry(ex)))
                {
                    break;
                }

                var delayToWait = CalculateDelay(currentDelay, attempt);
                await Task.Delay(delayToWait, _timeProvider, cancellationToken).ConfigureAwait(false);
            }
        }

        ResilienceDiagnostics.RecordStrategyExecution("retry", false);
        return VKResult.Failure<T>(VKResilienceErrors.CreateRetryExhausted(lastException?.Message));
    }

    /// <inheritdoc />
    public async Task<VKResult> ExecuteWithRetryAsync(
        Func<CancellationToken, Task> action,
        int? maxRetries = null,
        TimeSpan? initialDelay = null,
        Func<Exception, bool>? shouldRetry = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(action);

        int attemptsLimit = maxRetries ?? _options.MaxRetries;
        var currentDelay = initialDelay ?? _options.InitialDelay;
        Exception? lastException = null;

        for (int attempt = 0; attempt <= attemptsLimit; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                await action(cancellationToken).ConfigureAwait(false);
                ResilienceDiagnostics.RecordStrategyExecution("retry", true);
                return VKResult.Success();
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                lastException = ex;

                if (attempt == attemptsLimit || (shouldRetry != null && !shouldRetry(ex)))
                {
                    break;
                }

                var delayToWait = CalculateDelay(currentDelay, attempt);
                await Task.Delay(delayToWait, _timeProvider, cancellationToken).ConfigureAwait(false);
            }
        }

        ResilienceDiagnostics.RecordStrategyExecution("retry", false);
        return VKResult.Failure(VKResilienceErrors.CreateRetryExhausted(lastException?.Message));
    }

    private TimeSpan CalculateDelay(TimeSpan baseDelay, int attempt)
    {
        double multiplier = Math.Pow(_options.BackoffMultiplier, attempt);
        double delayMs = baseDelay.TotalMilliseconds * multiplier;

        if (_options.UseJitter)
        {
            // Apply +/- 20% jitter
            double jitter = (Random.Shared.NextDouble() * 0.4) + 0.8;
            delayMs *= jitter;
        }

        double cappedMs = Math.Min(delayMs, _options.MaxDelay.TotalMilliseconds);
        return TimeSpan.FromMilliseconds(cappedMs);
    }
}
