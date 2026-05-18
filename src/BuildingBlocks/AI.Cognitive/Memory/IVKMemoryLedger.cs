using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Memory: Responsible for short and long-term conversation accumulation and persistence.
/// Metaphor: Ledger - A record of life across time (The Chronicle).
/// Value: User preference storage (Industrial) and establishment of emotional bonds (PWP).
/// Handles factual recording, vector-based or key-value persistence.
/// </summary>
public interface IVKMemoryLedger
{
    /// <summary>
    /// Records a factual entry into the ledger.
    /// </summary>
    /// <param name="key">The key identifying the fact.</param>
    /// <param name="value">The factual data.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The result of the recording operation.</returns>
    Task<VKResult> RecordAsync(string key, object value, CancellationToken ct = default);

    /// <summary>
    /// Retrieves a factual entry from the ledger.
    /// </summary>
    /// <param name="key">The key identifying the fact.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A result containing the retrieved data.</returns>
    Task<VKResult<T?>> GetAsync<T>(string key, CancellationToken ct = default);
}
