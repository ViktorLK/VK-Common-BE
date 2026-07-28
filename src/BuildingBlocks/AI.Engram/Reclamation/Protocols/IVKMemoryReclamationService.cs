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
}
