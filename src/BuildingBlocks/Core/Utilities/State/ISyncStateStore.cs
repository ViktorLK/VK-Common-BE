using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core.Results;

namespace VK.Blocks.Core.Utilities.State;

/// <summary>
/// Defines a generic contract for storing and retrieving synchronization state (hashes/versions).
/// Used by Building Blocks to perform efficient conditional synchronization of metadata.
/// </summary>
public interface ISyncStateStore
{
    /// <summary>
    /// Retrieves the last stored hash/version for a specific synchronization key.
    /// </summary>
    /// <param name="key">The unique key for the synchronization task (e.g. "Permissions", "Menus").</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="ValueTask{TResult}"/> representing the asynchronous operation, containing the last stored hash, or <c>null</c> if it does not exist.</returns>
    ValueTask<string?> GetLastHashAsync(string key, CancellationToken ct = default);

    /// <summary>
    /// Persists a new hash/version for a specific synchronization key.
    /// </summary>
    /// <param name="key">The unique key for the synchronization task.</param>
    /// <param name="hash">The new hash to store.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="ValueTask{TResult}"/> representing the asynchronous operation, containing a <see cref="Result"/> indicating success or failure.</returns>
    ValueTask<Result> UpdateHashAsync(string key, string hash, CancellationToken ct = default);
}


