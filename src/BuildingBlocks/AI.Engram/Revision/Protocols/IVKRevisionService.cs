using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram;

/// <summary>
/// Service coordinating the revision of recalled memory items back into long term memory.
/// </summary>
public interface IVKRevisionService
{
    /// <summary>
    /// Processes active context and updates recalled long-term memories when modified.
    /// </summary>
    Task<VKResult> ReviseSessionMemoriesAsync(VKPsycheContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Explicitly processes a memory revision request with authority weighting and idempotency controls.
    /// </summary>
    Task<VKResult<VKContradictionArbitrationResult>> ReviseMemoryAsync(VKRevisionRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back a memory entry to a target prior version index.
    /// </summary>
    Task<VKResult> RollbackMemoryAsync(VKMemoryId memoryId, int targetVersion, CancellationToken cancellationToken = default);
}
