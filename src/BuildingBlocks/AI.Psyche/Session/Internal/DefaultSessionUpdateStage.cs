using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using VK.Blocks.AI.Psyche.Session.Diagnostics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Session.Internal;

/// <summary>
/// Pipeline stage running in the After phase to update session metrics (TurnCount, LastActivityAt) after successful execution.
/// Follows AP.01 (sealed class default), CS.01, CS.03, BB.04, and OR.01.
/// </summary>
[VKTrace("psyche.stage.session_update")]
internal sealed class DefaultSessionUpdateStage : IVKPsychePipelineStage
{
    private readonly VKSessionOptions _options;
    private readonly IVKPsycheSessionRepository _sessionRepository;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<DefaultSessionUpdateStage> _logger;

    public DefaultSessionUpdateStage(
        VKSessionOptions options,
        IVKPsycheSessionRepository sessionRepository,
        TimeProvider timeProvider,
        ILogger<DefaultSessionUpdateStage>? logger = null)
    {
        _options = VKGuard.NotNull(options);
        _sessionRepository = VKGuard.NotNull(sessionRepository);
        _timeProvider = VKGuard.NotNull(timeProvider);
        _logger = logger ?? NullLogger<DefaultSessionUpdateStage>.Instance;
    }

    public VKPipelineSchedule Schedule => VKPsychePipelineScheduler.After.PsycheSessionUpdate;

    public bool IsActive => _options.Enabled;

    public async Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(context);

        if (context.IsWeaveOnly || context.IsSandbox)
        {
            return VKResult.Success();
        }

        // Retrieve existing session thread resolved in the Before phase
        var session = context.State<VKSessionThread>();
        if (session is null)
        {
            return VKResult.Success();
        }

        var now = _timeProvider.GetUtcNow();
        var incResult = session.IncrementTurn(now);
        if (incResult.IsFailure)
        {
            return incResult;
        }

        var saveResult = await _sessionRepository.UpdateAsync(session, cancellationToken).ConfigureAwait(false);
        if (saveResult.IsFailure)
        {
            return saveResult;
        }

        _logger.SessionUpdated(session.Id, session.TurnCount);
        return saveResult;
    }
}
