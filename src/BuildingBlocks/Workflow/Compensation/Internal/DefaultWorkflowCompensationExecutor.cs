using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;
using VK.Blocks.Resilience;
using VK.Blocks.Workflow.Common.Diagnostics.Internal;

namespace VK.Blocks.Workflow.Compensation.Internal;

/// <summary>
/// Internal executor for running compensation handlers with resilient retry logic.
/// Follows AP.01, CS.01, CS.03, CS.06, OR.03.
/// </summary>
internal sealed class DefaultWorkflowCompensationExecutor
{
    private readonly IVKRetryExecutor _retryExecutor;
    private readonly IOptionsSnapshot<VKCompensationOptions> _options;
    private readonly ILogger<DefaultWorkflowCompensationExecutor> _logger;

    public DefaultWorkflowCompensationExecutor(
        IVKRetryExecutor retryExecutor,
        IOptionsSnapshot<VKCompensationOptions> options,
        ILogger<DefaultWorkflowCompensationExecutor> logger)
    {
        _retryExecutor = VKGuard.NotNull(retryExecutor);
        _options = VKGuard.NotNull(options);
        _logger = VKGuard.NotNull(logger);
    }

    public async Task<VKResult> ExecuteWithRetryAsync<TContext>(
        IVKWorkflowCompensationHandler<TContext> handler,
        TContext context,
        VKError originalError,
        CancellationToken cancellationToken)
    {
        VKGuard.NotNull(handler);
        cancellationToken.ThrowIfCancellationRequested();

        var maxRetries = Math.Max(1, _options.Value.MaxRetries);
        var baseDelay = TimeSpan.FromMilliseconds(Math.Max(10, _options.Value.RetryBaseDelayMs));
        var attemptCount = 0;

        var retryResult = await _retryExecutor.ExecuteWithRetryAsync(
            async ct =>
            {
                attemptCount++;
                var result = await handler.CompensateAsync(context, originalError, ct).ConfigureAwait(false);
                if (result.IsFailure)
                {
                    _logger.CompensationAttemptFailed(attemptCount, maxRetries, result.FirstError.Description);
                    throw new InvalidOperationException(result.FirstError.Description);
                }
            },
            maxRetries: maxRetries,
            initialDelay: baseDelay,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        if (retryResult.IsSuccess)
        {
            return VKResult.Success();
        }

        return VKResult.Failure(VKWorkflowErrors.CompensationFailed);
    }
}
