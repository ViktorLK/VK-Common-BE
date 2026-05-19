using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Defines the service responsible for pruning and compressing AI memories to model metabolic forgetting.
/// </summary>
public interface IVKMemoryPruner
{
    /// <summary>
    /// Executes the pruning and compression process for the specified tenant or global context.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<VKResult> RunPruningCycleAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Queues a pruning and compression cycle to run asynchronously in the background.
    /// Does not block or wait for completion.
    /// Default implementation does nothing to maintain backward compatibility.
    /// </summary>
    void QueuePruningCycle() { }
}
