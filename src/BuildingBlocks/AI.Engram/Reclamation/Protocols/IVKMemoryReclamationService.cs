using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram;

/// <summary>
/// Service coordinating memory reclamation (decay calculation, low-score pruning, and vector store cleanup).
/// </summary>
public interface IVKMemoryReclamationService
{
    /// <summary>
    /// Executes a full memory reclamation cycle (Decay -> Prune -> VectorStore Cleanup).
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing statistics of the reclamation cycle.</returns>
    Task<VKResult<VKReclamationResult>> RunReclamationCycleAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a memory reclamation cycle with specific run options (e.g., DryRun, batch size overrides).
    /// </summary>
    /// <param name="runOptions">Execution options for this specific run.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing statistics and detailed prune audit details.</returns>
    Task<VKResult<VKReclamationResult>> RunReclamationCycleAsync(VKReclamationRunOptions runOptions, CancellationToken cancellationToken = default);
}
