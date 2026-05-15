using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Defines the service responsible for pruning and compressing AI memories.
/// </summary>
public interface IVKMemoryPruningService
{
    /// <summary>
    /// Executes the pruning and compression process for the specified tenant or global context.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<VKResult> RunPruningCycleAsync(CancellationToken cancellationToken = default);
}
