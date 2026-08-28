using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Session.Internal;

/// <summary>
/// Pipeline stage running in the After phase to update session metrics (TurnCount, LastActivityAt) after successful execution.
/// Follows AP.01 (sealed class default), CS.01, and CS.03.
/// </summary>
internal sealed class DefaultSessionUpdateStage : IVKPsychePipelineStage
{
    private readonly VKSessionOptions _options;
    private readonly IVKSessionStore _sessionStore;
    private readonly TimeProvider _timeProvider;

    public DefaultSessionUpdateStage(
        VKSessionOptions options,
        IVKSessionStore sessionStore,
        TimeProvider timeProvider)
    {
        _options = VKGuard.NotNull(options);
        _sessionStore = VKGuard.NotNull(sessionStore);
        _timeProvider = VKGuard.NotNull(timeProvider);
    }

    public VKPipelineSchedule Schedule => VKPsychePipelineScheduler.After.PsycheSessionUpdate;

    public bool IsActive => _options.Enabled;

    public async Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(context);

        var stopwatch = Stopwatch.StartNew();
        try
        {
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

            var saveResult = await _sessionStore.UpdateSessionAsync(session, cancellationToken).ConfigureAwait(false);
            return saveResult;
        }
        finally
        {
            stopwatch.Stop();
            context.ResponseBuilder.ProfilingMetrics[VKPsycheProfilingKeys.SessionUpdateStage] = stopwatch.Elapsed.TotalMilliseconds;
        }
    }
}
