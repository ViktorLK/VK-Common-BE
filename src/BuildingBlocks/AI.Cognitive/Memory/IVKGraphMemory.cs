using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Defines the interface for a knowledge graph memory layer.
/// Handles relationships between entities for complex reasoning (PWP08).
/// </summary>
public interface IVKGraphMemory
{
    /// <summary>
    /// Adds or updates a relationship between two entities.
    /// </summary>
    Task<VKResult> RelateAsync(string sourceId, string relationship, string targetId, IDictionary<string, object?>? properties = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves entities related to the source entity.
    /// </summary>
    Task<VKResult<IEnumerable<string>>> GetRelatedAsync(string sourceId, string? relationship = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds paths between two entities.
    /// </summary>
    Task<VKResult<IEnumerable<string>>> FindPathAsync(string startId, string endId, int maxDepth = 3, CancellationToken cancellationToken = default);
}
