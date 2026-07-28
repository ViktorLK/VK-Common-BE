using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram;

/// <summary>
/// Defines the interface for a structured behavior/fact memory layer.
/// Handles deterministic key-value data and schema/type-checked facts that remain immune to decay.
/// </summary>
public interface IVKStructuredMemoryStore
{
    /// <summary>
    /// Stores a structured fact or behavioral setting with optional schema/type checking and sensitivity tagging.
    /// </summary>
    Task<VKResult> StoreFactAsync(
        string key,
        object value,
        Type? expectedType = null,
        bool isSensitive = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a structured fact by key.
    /// </summary>
    Task<VKResult<T>> GetFactAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a fact exists within the current scope.
    /// </summary>
    Task<VKResult<bool>> HasFactAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Explicitly removes a structured fact by key (GDPR / Right-to-forget).
    /// </summary>
    Task<VKResult> RemoveFactAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Explicitly removes all structured facts matching a prefix within the current scope.
    /// </summary>
    Task<VKResult<int>> RemoveAllFactsAsync(string? prefix = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all fact keys within a specific namespace or prefix in current scope.
    /// </summary>
    Task<VKResult<IEnumerable<string>>> ListKeysAsync(string? prefix = null, CancellationToken cancellationToken = default);
}
