using System;
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

    public VKPipelineSchedule Schedule => VKPsychePipelineScheduler.After.SessionUpdate;
    public bool IsActive => _options.Enabled;

    public async Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(context);

        // Retrieve existing session thread resolved in the Before phase
        var session = context.State<VKSessionThread>();
        if (session is null)
        {
            return VKResult.Success();
        }

        var now = _timeProvider.GetUtcNow();
        var updatedSession = session with
        {
            TurnCount = session.TurnCount + 1,
            LastActivityAt = now,
            UpdatedAt = now
        };

        var saveResult = await _sessionStore.SaveSessionAsync(updatedSession, cancellationToken).ConfigureAwait(false);
        if (saveResult.IsSuccess)
        {
            context.SetState(updatedSession);
        }

        return saveResult;
    }
}
