using System;
using System.Threading;
using System.Threading.Tasks;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Defines the contract for recording system performance telemetry metrics and identifying stressed providers.
/// Follows CS.03 and AP.03.
/// </summary>
public interface IVKSystemTelemetry
{
    /// <summary>
    /// Records the execution latency and outcome for a downstream provider request.
    /// </summary>
    /// <param name="provider">The provider name (e.g. "AzureOpenAI").</param>
    /// <param name="latency">The measured duration.</param>
    /// <param name="isSuccess">Whether the request completed successfully.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RecordLatencyAsync(
        string provider,
        TimeSpan latency,
        bool isSuccess,
        CancellationToken cancellationToken = default); // [CS.03]

    /// <summary>
    /// Checks whether the specified provider is currently experiencing stress or high failure rates.
    /// </summary>
    /// <param name="provider">The provider name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A boolean indicating whether the provider is currently stressed.</returns>
    Task<bool> IsProviderStressedAsync(
        string provider,
        CancellationToken cancellationToken = default); // [CS.03]
}
