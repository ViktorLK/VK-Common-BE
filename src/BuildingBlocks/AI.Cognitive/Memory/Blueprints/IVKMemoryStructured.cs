using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Defines the interface for a structured behavior/fact memory layer.
/// Handles deterministic key-value data and schema-based facts.
/// </summary>
public interface IVKMemoryStructured
{
    /// <summary>
    /// Stores a structured fact or behavioral setting.
    /// </summary>
    Task<VKResult> StoreFactAsync(string key, object value, string? schema = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a structured fact by key.
    /// </summary>
    Task<VKResult<T>> GetFactAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a fact exists.
    /// </summary>
    Task<VKResult<bool>> HasFactAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all facts within a specific namespace or prefix.
    /// </summary>
    Task<VKResult<IEnumerable<string>>> ListKeysAsync(string? prefix = null, CancellationToken cancellationToken = default);
}
