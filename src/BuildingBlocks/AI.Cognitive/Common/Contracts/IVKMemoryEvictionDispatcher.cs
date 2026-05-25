using System.Threading;
using System.Threading.Tasks;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Defines the dispatcher boundary for conversation memory evictions.
/// Exposes a clean, asynchronous closed-loop dispatch contract.
/// Follows CS.03 (asynchronous + CancellationToken support) and AP.03.
/// </summary>
public interface IVKMemoryEvictionDispatcher
{
    /// <summary>
    /// Dispatches the memory eviction event asynchronously.
    /// </summary>
    /// <param name="event">The memory eviction event.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A ValueTask representing the asynchronous operation.</returns>
    ValueTask DispatchAsync(VKMemoryEvictionEvent @event, CancellationToken cancellationToken = default);
}
