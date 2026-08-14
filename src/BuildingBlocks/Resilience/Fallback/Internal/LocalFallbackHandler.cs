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
        Func<CancellationToken, Task<T>> primaryAction,
        Func<Exception, CancellationToken, Task<T>> fallbackAction,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(primaryAction);
        VKGuard.NotNull(fallbackAction);

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
                ResilienceDiagnostics.RecordStrategyExecution("fallback", true);
                return VKResult.Success(fallbackResult);
            }
            catch (Exception fallbackEx)
            {
                ResilienceDiagnostics.RecordStrategyExecution("fallback", false);
                return VKResult.Failure<T>(VKResilienceErrors.CreateFallbackFailed(fallbackEx.Message));
            }
        }
    }
}
