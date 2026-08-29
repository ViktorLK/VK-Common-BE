using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using VK.Blocks.AI.Psyche.Session.Diagnostics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Session.Internal;

/// <summary>
/// Pipeline stage responsible for resolving and attaching <see cref="VKSessionThread"/> metadata before prompt weaving.
/// Follows AP.01 (sealed class default), CS.01, CS.03, BB.04, and OR.01.
/// </summary>
[VKTrace("psyche.stage.session_resolve")]
internal sealed class DefaultSessionResolveStage : IVKPsychePipelineStage
{
    private readonly VKSessionOptions _options;
    private readonly IVKPsycheSessionRepository _sessionRepository;
    private readonly ILogger<DefaultSessionResolveStage> _logger;

    public DefaultSessionResolveStage(
        VKSessionOptions options,
        IVKPsycheSessionRepository sessionRepository,
        ILogger<DefaultSessionResolveStage>? logger = null)
    {
        _options = VKGuard.NotNull(options);
        _sessionRepository = VKGuard.NotNull(sessionRepository);
        _logger = logger ?? NullLogger<DefaultSessionResolveStage>.Instance;
    }

    public VKPipelineSchedule Schedule => VKPsychePipelineScheduler.Before.PsycheSessionResolve;

    public bool IsActive => _options.Enabled;

    public async Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(context);

        // 1. Guard against empty SessionId (stateless call)
        if (context.Request.SessionId.IsEmpty)
        {
            return VKResult.Success();
        }

        var resolveResult = await _sessionRepository.FindByIdAsync(context.Request.SessionId, cancellationToken).ConfigureAwait(false);
        if (resolveResult.IsSuccess && resolveResult.Value is not null)
        {
            var session = resolveResult.Value;
            if (session.Status != VKSessionStatus.Active)
            {
                _logger.SessionNotActive(session.Id, session.Status.ToString());
                return VKResult.Failure(VKSessionErrors.SessionNotActive);
            }

            context.SetState(session);
            _logger.SessionResolved(session.Id, session.Mode.ToString(), session.TurnCount);
            SessionDiagnostics.RecordActiveSessionsResolved(1, "SessionResolve");
        }

        return VKResult.Success();
    }
}
