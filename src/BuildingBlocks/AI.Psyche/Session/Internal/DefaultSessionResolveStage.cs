using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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
    public string TraceName => "psyche.stage.session_resolve";
    public string StageName => "SessionResolve";

    private readonly VKSessionOptions _options;
    private readonly IVKPsycheSessionRepository _sessionRepository;
    private readonly ILogger<DefaultSessionResolveStage> _logger;

    public DefaultSessionResolveStage(
        VKSessionOptions options,
        IVKPsycheSessionRepository sessionRepository,
        ILogger<DefaultSessionResolveStage> logger)
    {
        _options = VKGuard.NotNull(options);
        _sessionRepository = VKGuard.NotNull(sessionRepository);
        _logger = VKGuard.NotNull(logger);
    }

    public VKPipelineSchedule Schedule => VKPsychePipelineScheduler.Before.PsycheSessionResolve;

    public bool IsActive => _options.Enabled;

    public async Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(context);

        // 1. Guard against empty SessionId (stateless call per AP.07)
        if (context.Request.SessionId.IsEmpty)
        {
            return VKResult.Success();
        }

        var stopwatch = Stopwatch.StartNew();
        var resolveResult = await _sessionRepository.FindByIdAsync(context.Request.SessionId, cancellationToken).ConfigureAwait(false); // [CS.03]
        stopwatch.Stop();
        var durationMs = stopwatch.Elapsed.TotalMilliseconds;

        // 2. [AP.06 / CS.01] Fail-Fast when explicit identifier lookup fails
        if (resolveResult.IsFailure)
        {
            SessionDiagnostics.RecordSessionResolve(durationMs, StageName, success: false);
            return VKResult.Failure(resolveResult.Errors);
        }

        var session = resolveResult.Value;
        if (session is null)
        {
            SessionDiagnostics.RecordSessionResolve(durationMs, StageName, success: false);
            return VKResult.Failure(VKSessionErrors.NotFound);
        }
        if (session.Status != VKSessionStatus.Active)
        {
            _logger.SessionNotActive(session.Id, session.Status);
            SessionDiagnostics.RecordSessionResolve(durationMs, StageName, success: false);
            return VKResult.Failure(VKSessionErrors.SessionNotActive);
        }

        if (session.Mode == VKSessionMode.Continuous
            && session.ParentSessionId.HasValue
            && session.BaseTurnOffset == 0
            && session.TurnCount == 0
            && !session.ForkPointEchoId.HasValue)
        {
            var parentResolveResult = await _sessionRepository.FindByIdAsync(session.ParentSessionId.Value, cancellationToken).ConfigureAwait(false);
            if (parentResolveResult.IsSuccess && parentResolveResult.Value is not null && parentResolveResult.Value.AbsoluteTurnCount > 0)
            {
                session.SetBaseTurnOffset(parentResolveResult.Value.AbsoluteTurnCount);
            }
        }

        context.SetState(session);
        _logger.SessionResolved(session.Id, session.Mode, session.AbsoluteTurnCount);
        SessionDiagnostics.RecordSessionResolve(durationMs, StageName, success: true);
        SessionDiagnostics.RecordActiveSessionsResolved(1, StageName);

        return VKResult.Success();
    }
}
