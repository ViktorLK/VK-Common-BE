using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Session.Internal;

/// <summary>
/// Pipeline stage responsible for resolving and attaching <see cref="VKSessionThread"/> metadata before prompt weaving.
/// Follows AP.01 (sealed class default), CS.01, and CS.03.
/// </summary>
internal sealed class DefaultSessionStage : IVKPsychePipelineStage
{
    private readonly IVKSessionStore _sessionStore;

    public DefaultSessionStage(IVKSessionStore sessionStore)
    {
        _sessionStore = VKGuard.NotNull(sessionStore);
    }

    public VKPipelineSchedule Schedule => VKPsychePipelineScheduler.Before.PsycheSession;
    public bool IsActive => true;

    public async Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(context);

        // Resolve existing session thread metadata if SessionId is provided in request
        if (!context.Request.SessionId.IsNullOrEmpty())
        {
            var resolveResult = await _sessionStore.GetSessionAsync(context.Request.SessionId, cancellationToken).ConfigureAwait(false);
            if (resolveResult.IsSuccess && resolveResult.Value is not null)
            {
                context.SetState(resolveResult.Value);
            }
        }

        return VKResult.Success();
    }
}
