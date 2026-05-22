using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Defines the contract for an external presence contributor.
/// Implementers (e.g. in AI.Somatic or AI.Social) register themselves in DI to inject custom metadata overlays.
/// Follows CS.03 (CancellationToken support) and AP.03.
/// </summary>
public interface IVKPresenceContributor
{
    /// <summary>
    /// Gets the formatting and execution priority (lower values compile first).
    /// </summary>
    int Priority { get; }

    /// <summary>
    /// Contributes custom awareness or relational coordinates to the active session.
    /// </summary>
    /// <param name="context">The presence contribution context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the contribution data.</returns>
    Task<VKResult<VKPresenceContribution>> ContributeAsync(
        VKPresenceContributionContext context,
        CancellationToken cancellationToken = default); // [CS.03]
}
