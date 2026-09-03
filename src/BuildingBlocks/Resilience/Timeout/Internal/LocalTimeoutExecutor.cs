using System;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;
using VK.Blocks.Resilience.Diagnostics.Internal;

namespace VK.Blocks.Resilience.Timeout.Internal;

// [AP.01] sealed
internal sealed class LocalTimeoutExecutor : IVKTimeoutExecutor
{
    private readonly VKTimeoutOptions _options;

    public LocalTimeoutExecutor(VKTimeoutOptions options)
    {
        _options = VKGuard.NotNull(options);
    }

    /// <inheritdoc />
    public async Task<VKResult<T>> ExecuteWithTimeoutAsync<T>(
        Func<CancellationToken, Task<VKResult<T>>> action,
        TimeSpan? timeout = null,
        bool isPessimistic = false,
        Action<TimeSpan>? onTimeout = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(action);
        var effectiveTimeout = timeout ?? _options.Duration;

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(effectiveTimeout);

        if (!isPessimistic)
        {
            // Optimistic (Cooperative Cancellation)
            try
            {
                var result = await action(cts.Token).ConfigureAwait(false);
                ResilienceDiagnostics.RecordStrategyExecution("timeout", result.IsSuccess);
                return result;
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested && cts.IsCancellationRequested)
            {
                ResilienceDiagnostics.RecordStrategyExecution("timeout", false);
                onTimeout?.Invoke(effectiveTimeout);
                return VKResult.Failure<T>(VKResilienceErrors.CreateTimeout(effectiveTimeout.TotalMilliseconds));
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                ResilienceDiagnostics.RecordStrategyExecution("timeout", false);
                return VKResult.Failure<T>(VKResilienceErrors.CreateExecutionFailed(ex.Message));
            }
        }
        else
        {
            // Pessimistic (Task Abandonment / WhenAny)
            try
            {
                var actionTask = action(cts.Token);
                var timeoutTask = Task.Delay(effectiveTimeout, cancellationToken);

                var completedTask = await Task.WhenAny(actionTask, timeoutTask).ConfigureAwait(false);
                if (completedTask == actionTask)
                {
                    var result = await actionTask.ConfigureAwait(false);
                    ResilienceDiagnostics.RecordStrategyExecution("timeout", result.IsSuccess);
                    return result;
                }

                // Timed out
                cts.Cancel(); // signal cancellation to background worker
                ResilienceDiagnostics.RecordStrategyExecution("timeout", false);
                onTimeout?.Invoke(effectiveTimeout);
                return VKResult.Failure<T>(VKResilienceErrors.CreateTimeout(effectiveTimeout.TotalMilliseconds));
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                ResilienceDiagnostics.RecordStrategyExecution("timeout", false);
                return VKResult.Failure<T>(VKResilienceErrors.CreateExecutionFailed(ex.Message));
            }
        }
    }

    /// <inheritdoc />
    public async Task<VKResult> ExecuteWithTimeoutAsync(
        Func<CancellationToken, Task<VKResult>> action,
        TimeSpan? timeout = null,
        bool isPessimistic = false,
        Action<TimeSpan>? onTimeout = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(action);
        var effectiveTimeout = timeout ?? _options.Duration;

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(effectiveTimeout);

        if (!isPessimistic)
        {
            try
            {
                var result = await action(cts.Token).ConfigureAwait(false);
                ResilienceDiagnostics.RecordStrategyExecution("timeout", result.IsSuccess);
                return result;
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested && cts.IsCancellationRequested)
            {
                ResilienceDiagnostics.RecordStrategyExecution("timeout", false);
                onTimeout?.Invoke(effectiveTimeout);
                return VKResult.Failure(VKResilienceErrors.CreateTimeout(effectiveTimeout.TotalMilliseconds));
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                ResilienceDiagnostics.RecordStrategyExecution("timeout", false);
                return VKResult.Failure(VKResilienceErrors.CreateExecutionFailed(ex.Message));
            }
        }
        else
        {
            try
            {
                var actionTask = action(cts.Token);
                var timeoutTask = Task.Delay(effectiveTimeout, cancellationToken);

                var completedTask = await Task.WhenAny(actionTask, timeoutTask).ConfigureAwait(false);
                if (completedTask == actionTask)
                {
                    var result = await actionTask.ConfigureAwait(false);
                    ResilienceDiagnostics.RecordStrategyExecution("timeout", result.IsSuccess);
                    return result;
                }

                cts.Cancel();
                ResilienceDiagnostics.RecordStrategyExecution("timeout", false);
                onTimeout?.Invoke(effectiveTimeout);
                return VKResult.Failure(VKResilienceErrors.CreateTimeout(effectiveTimeout.TotalMilliseconds));
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                ResilienceDiagnostics.RecordStrategyExecution("timeout", false);
                return VKResult.Failure(VKResilienceErrors.CreateExecutionFailed(ex.Message));
            }
        }
    }

    /// <inheritdoc />
    public async Task<VKResult<T>> ExecuteWithTimeoutAsync<T>(
        Func<CancellationToken, Task<T>> action,
        TimeSpan? timeout = null,
        bool isPessimistic = false,
        Action<TimeSpan>? onTimeout = null,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteWithTimeoutAsync(
            async ct =>
            {
                var val = await action(ct).ConfigureAwait(false);
                return VKResult.Success(val);
            },
            timeout,
            isPessimistic,
            onTimeout,
            cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<VKResult> ExecuteWithTimeoutAsync(
        Func<CancellationToken, Task> action,
        TimeSpan? timeout = null,
        bool isPessimistic = false,
        Action<TimeSpan>? onTimeout = null,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteWithTimeoutAsync(
            async ct =>
            {
                await action(ct).ConfigureAwait(false);
                return VKResult.Success();
            },
            timeout,
            isPessimistic,
            onTimeout,
            cancellationToken).ConfigureAwait(false);
    }
}
