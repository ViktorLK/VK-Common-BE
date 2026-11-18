using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Defines a policy that resolves candidate AI connections for a routing request.
/// </summary>
// [ARCH-02] [CS.01] [CS.03]
public interface IVKAIRoutingPolicy
{
    /// <summary>
    /// Decides the ordered list of candidate AI connections for the given request.
    /// </summary>
    /// <param name="request">The routing request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the routing decision.</returns>
    ValueTask<VKResult<VKAIRouteDecision>> DecideAsync(
        VKAIRouteRequest request,
        CancellationToken cancellationToken = default);
}
