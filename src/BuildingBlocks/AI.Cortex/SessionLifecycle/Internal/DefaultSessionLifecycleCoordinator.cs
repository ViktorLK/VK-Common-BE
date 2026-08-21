using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.AI.Cortex.Common.Diagnostics.Internal;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cortex.SessionLifecycle.Internal;

/// <summary>
/// Default industrial coordinator for session boundary evaluation and post-session consolidation dispatch.
/// Follows AP.01, CS.01, CS.03, CS.06, CS.07, OR.01.
/// </summary>
internal sealed class DefaultSessionLifecycleCoordinator : IVKSessionLifecycleCoordinator
{
    private readonly TimeProvider _timeProvider;
    private readonly IOptionsSnapshot<VKSessionLifecycleOptions> _options;
    private readonly ILogger<DefaultSessionLifecycleCoordinator> _logger;
    private readonly IEnumerable<IVKSessionEndedHandler>? _handlers;

    public DefaultSessionLifecycleCoordinator(
        TimeProvider timeProvider,
        IOptionsSnapshot<VKSessionLifecycleOptions> options,
        ILogger<DefaultSessionLifecycleCoordinator> logger,
        IEnumerable<IVKSessionEndedHandler>? handlers = null)
    {
        _timeProvider = VKGuard.NotNull(timeProvider);
        _options = VKGuard.NotNull(options);
        _logger = VKGuard.NotNull(logger);
        _handlers = handlers;
    }

    /// <inheritdoc />
    public bool IsSessionExpired(DateTimeOffset lastActivityAt)
    {
        var now = _timeProvider.GetUtcNow();
        var idleDuration = now - lastActivityAt;

        if (idleDuration >= _options.Value.IdleTimeout)
        {
            return true;
        }

        if (_options.Value.EnableCrossDayBoundary && now.UtcDateTime.Date > lastActivityAt.UtcDateTime.Date)
        {
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public async Task<VKResult> OnSessionEndedAsync(
        VKSessionId sessionId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _logger.SessionBoundaryTriggered(sessionId.ToString());

        try
        {
            if (_handlers is not null)
            {
                foreach (var handler in _handlers)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var handlerResult = await handler.OnSessionEndedAsync(sessionId, cancellationToken).ConfigureAwait(false);
                    if (handlerResult.IsFailure)
                    {
                        _logger.SessionConsolidationStepFailed(sessionId.ToString(), handler.GetType().Name, handlerResult.FirstError.Description);
                    }
                }
            }

            _logger.SessionConsolidationCompleted(sessionId.ToString());
            return VKResult.Success();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.SessionConsolidationStepFailed(sessionId.ToString(), "ConsolidationDispatch", ex.Message);
            return VKResult.Failure(VKCortexErrors.ConsolidationFailed);
        }
    }
}
