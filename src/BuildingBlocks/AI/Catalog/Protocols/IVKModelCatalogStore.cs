using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Persistence store interface for querying and loading custom AI model definitions.
/// Follows CS.01 (Result pattern) and CS.03 (CancellationToken).
/// </summary>
public interface IVKModelCatalogStore
{
    /// <summary>
    /// Loads all custom model metadata from the underlying storage.
    /// </summary>
    Task<VKResult<IReadOnlyList<VKModelMetadata>>> GetAllAsync(CancellationToken cancellationToken = default);
}
