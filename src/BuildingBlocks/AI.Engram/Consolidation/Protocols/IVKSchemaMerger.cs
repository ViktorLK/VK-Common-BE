using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram.Consolidation;

/// <summary>
/// Merges new facts into existing memory schemas or user profiles.
/// </summary>
public interface IVKSchemaMerger
{
    /// <summary>
    /// Merges new facts into the existing schema content according to the conflict strategy.
    /// </summary>
    Task<VKResult<string>> MergeSchemaAsync(
        string? existingSchema,
        string newFacts,
        VKConsolidationConflictStrategy strategy,
        CancellationToken cancellationToken = default);
}
