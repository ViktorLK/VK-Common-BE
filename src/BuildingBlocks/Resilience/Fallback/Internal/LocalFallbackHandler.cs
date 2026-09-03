using System;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;
using VK.Blocks.Resilience.Diagnostics.Internal;

namespace VK.Blocks.Resilience.Fallback.Internal;

// [AP.01] sealed
internal sealed class LocalFallbackHandler : IVKFallbackHandler
{
    public async Task<VKResult<T>> ExecuteWithFallbackAsync<T>(
        Func<CancellationToken, Task<VKResult<T>>> primaryAction,
        Func<VKError, CancellationToken, Task<VKResult<T>>> fallbackAction,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(primaryAction);
        VKGuard.NotNull(fallbackAction);
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var primaryResult = await primaryAction(cancellationToken).ConfigureAwait(false);
            if (primaryResult.IsSuccess)
            {
                ResilienceDiagnostics.RecordStrategyExecution("fallback_primary", true);
                return primaryResult;
            }

            var fallbackResult = await fallbackAction(primaryResult.FirstError, cancellationToken).ConfigureAwait(false);
            ResilienceDiagnostics.RecordStrategyExecution("fallback_secondary", fallbackResult.IsSuccess);
            return fallbackResult;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            try
            {
                var fallbackResult = await fallbackAction(VKResilienceErrors.CreateExecutionFailed(ex.Message), cancellationToken).ConfigureAwait(false);
                ResilienceDiagnostics.RecordStrategyExecution("fallback_secondary", fallbackResult.IsSuccess);
                return fallbackResult;
            }
            catch (Exception fallbackEx)
            {
                ResilienceDiagnostics.RecordStrategyExecution("fallback_secondary", false);
                return VKResult.Failure<T>(VKResilienceErrors.CreateFallbackFailed(fallbackEx.Message));
            }
        }
    }

    public async Task<VKResult> ExecuteWithFallbackAsync(
        Func<CancellationToken, Task<VKResult>> primaryAction,
        Func<VKError, CancellationToken, Task<VKResult>> fallbackAction,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(primaryAction);
        VKGuard.NotNull(fallbackAction);
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var primaryResult = await primaryAction(cancellationToken).ConfigureAwait(false);
            if (primaryResult.IsSuccess)
            {
                ResilienceDiagnostics.RecordStrategyExecution("fallback_primary", true);
                return primaryResult;
            }

            var fallbackResult = await fallbackAction(primaryResult.FirstError, cancellationToken).ConfigureAwait(false);
            ResilienceDiagnostics.RecordStrategyExecution("fallback_secondary", fallbackResult.IsSuccess);
            return fallbackResult;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            try
            {
                var fallbackResult = await fallbackAction(VKResilienceErrors.CreateExecutionFailed(ex.Message), cancellationToken).ConfigureAwait(false);
                ResilienceDiagnostics.RecordStrategyExecution("fallback_secondary", fallbackResult.IsSuccess);
                return fallbackResult;
            }
            catch (Exception fallbackEx)
            {
                ResilienceDiagnostics.RecordStrategyExecution("fallback_secondary", false);
                return VKResult.Failure(VKResilienceErrors.CreateFallbackFailed(fallbackEx.Message));
            }
        }
    }

    public async Task<VKResult<T>> ExecuteWithFallbackAsync<T>(
        Func<CancellationToken, Task<T>> primaryAction,
        Func<Exception, CancellationToken, Task<T>> fallbackAction,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(primaryAction);
        VKGuard.NotNull(fallbackAction);
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var primaryResult = await primaryAction(cancellationToken).ConfigureAwait(false);
            ResilienceDiagnostics.RecordStrategyExecution("fallback_primary", true);
            return VKResult.Success(primaryResult);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            try
            {
                var fallbackResult = await fallbackAction(ex, cancellationToken).ConfigureAwait(false);
                ResilienceDiagnostics.RecordStrategyExecution("fallback_secondary", true);
                return VKResult.Success(fallbackResult);
            }
            catch (Exception fallbackEx)
            {
                ResilienceDiagnostics.RecordStrategyExecution("fallback_secondary", false);
                return VKResult.Failure<T>(VKResilienceErrors.CreateFallbackFailed(fallbackEx.Message));
            }
        }
    }

    public async Task<VKResult> ExecuteWithFallbackAsync(
        Func<CancellationToken, Task> primaryAction,
        Func<Exception, CancellationToken, Task> fallbackAction,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(primaryAction);
        VKGuard.NotNull(fallbackAction);
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            await primaryAction(cancellationToken).ConfigureAwait(false);
            ResilienceDiagnostics.RecordStrategyExecution("fallback_primary", true);
            return VKResult.Success();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            try
            {
                await fallbackAction(ex, cancellationToken).ConfigureAwait(false);
                ResilienceDiagnostics.RecordStrategyExecution("fallback_secondary", true);
                return VKResult.Success();
            }
            catch (Exception fallbackEx)
            {
                ResilienceDiagnostics.RecordStrategyExecution("fallback_secondary", false);
                return VKResult.Failure(VKResilienceErrors.CreateFallbackFailed(fallbackEx.Message));
            }
        }
    }
}
