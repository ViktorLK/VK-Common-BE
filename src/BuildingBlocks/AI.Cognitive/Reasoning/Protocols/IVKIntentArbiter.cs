using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Defines the interface for resolving conflicts or selecting the primary intent from multiple candidates.
/// </summary>
public interface IVKIntentArbiter
{
    /// <summary>
    /// Arbitrates between candidate intents to determine the most appropriate one to act upon.
    /// </summary>
    /// <param name="candidates">The detected candidate intents.</param>
    /// <param name="args">The arbitration arguments.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the winning intent(s) or a resolution context.</returns>
    Task<VKResult<VKIntentContext>> ArbitrateAsync(
        IEnumerable<VKIntent> candidates,
        VKIntentArbiterArgs? args = null,
        CancellationToken cancellationToken = default);
}
