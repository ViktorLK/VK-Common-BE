using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Session.Internal;

/// <summary>
/// Pipeline stage responsible for resolving and attaching <see cref="VKSessionThread"/> metadata before prompt weaving.
/// Follows AP.01 (sealed class default), CS.01, and CS.03.
/// </summary>
internal sealed class DefaultSessionResolveStage : IVKPsychePipelineStage
{
    private readonly VKSessionOptions _options;
    private readonly IVKSessionStore _sessionStore;

    public DefaultSessionResolveStage(
        VKSessionOptions options,
        IVKSessionStore sessionStore)
    {
        _options = VKGuard.NotNull(options);
        _sessionStore = VKGuard.NotNull(sessionStore);
    }

    public VKPipelineSchedule Schedule => VKPsychePipelineScheduler.Before.PsycheSessionResolve;

    public bool IsActive => _options.Enabled;

    public async Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(context);

        var stopwatch = Stopwatch.StartNew();
        try
        {
            // 1. Guard against empty SessionId (stateless call)
            if (context.Request.SessionId.IsEmpty)
            {
                return VKResult.Success();
            }

            var resolveResult = await _sessionStore.GetSessionAsync(context.Request.SessionId, cancellationToken).ConfigureAwait(false);
            if (resolveResult.IsSuccess && resolveResult.Value is not null)
            {
                var session = resolveResult.Value;
                if (session.Status != VKSessionStatus.Active)
                {
                    return VKResult.Failure(VKSessionErrors.SessionNotActive);
                }

                context.SetState(session);
            }

            return VKResult.Success();
        }
        finally
        {
            stopwatch.Stop();
            context.ResponseBuilder.ProfilingMetrics[VKPsycheProfilingKeys.SessionResolveStage] = stopwatch.Elapsed.TotalMilliseconds;
        }
    }
}
