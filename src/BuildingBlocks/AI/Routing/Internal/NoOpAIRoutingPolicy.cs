using System;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Routing.Internal;

/// <summary>
/// Default no-op implementation of <see cref="IVKAIRoutingPolicy"/> returning an empty decision.
/// </summary>
// [ARCH-05] [AP.01]
internal sealed class NoOpAIRoutingPolicy : IVKAIRoutingPolicy
{
    private static readonly VKAIRouteDecision EmptyDecision = new()
    {
        Candidates = Array.Empty<VKAIRouteCandidate>(),
        MaxFailoverAttempts = 0,
        AllowCrossProviderFailover = false
    };

    /// <inheritdoc />
    public ValueTask<VKResult<VKAIRouteDecision>> DecideAsync(
        VKAIRouteRequest request,
        CancellationToken cancellationToken = default)
        => ValueTask.FromResult(VKResult.Success(EmptyDecision));
}
