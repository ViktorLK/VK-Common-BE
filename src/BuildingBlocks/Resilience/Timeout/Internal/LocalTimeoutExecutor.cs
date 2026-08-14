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

    public async Task<VKResult<T>> ExecuteWithTimeoutAsync<T>(
        Func<CancellationToken, Task<T>> action,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(action);
        var effectiveTimeout = timeout ?? _options.Duration;

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(effectiveTimeout);

        try
        {
            var result = await action(cts.Token).ConfigureAwait(false);
            ResilienceDiagnostics.RecordStrategyExecution("timeout", true);
            return VKResult.Success(result);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested && cts.IsCancellationRequested)
        {
            ResilienceDiagnostics.RecordStrategyExecution("timeout", false);
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

    public async Task<VKResult> ExecuteWithTimeoutAsync(
        Func<CancellationToken, Task> action,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(action);
        var effectiveTimeout = timeout ?? _options.Duration;

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(effectiveTimeout);

        try
        {
            await action(cts.Token).ConfigureAwait(false);
            ResilienceDiagnostics.RecordStrategyExecution("timeout", true);
            return VKResult.Success();
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested && cts.IsCancellationRequested)
        {
            ResilienceDiagnostics.RecordStrategyExecution("timeout", false);
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
