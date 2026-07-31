using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram;

/// <summary>
/// Service for manually overriding BaseImportance of existing memory entries.
/// </summary>
public interface IVKScoreOverrideService
{
    /// <summary>
    /// Manually overrides the BaseImportance of an existing memory entry.
    /// Distinct from content revision, this explicitly updates cognitive weight.
    /// </summary>
    Task<VKResult> OverrideBaseImportanceAsync(
        VKMemoryId memoryId,
        double newBaseImportance,
        CancellationToken cancellationToken = default);
}
