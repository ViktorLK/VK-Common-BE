using System;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.Resilience;

/// <summary>
/// Defines the contract for token-bucket rate limiting allowing smooth replenishment and controlled burst handling.
/// Follows [AP.01], [CS.01], [CS.03].
/// </summary>
public interface IVKTokenBucketLimiter
{
    /// <summary>
    /// Checks if the requested number of tokens can be acquired immediately without waiting.
    /// </summary>
    bool TryAcquire(string key, double tokens = 1.0);

    /// <summary>
    /// Attempts to acquire tokens asynchronously, optionally waiting up to <paramref name="maxWaitDuration"/>.
    /// </summary>
    Task<VKResult> AcquireAsync(
        string key,
        double tokens = 1.0,
        TimeSpan? maxWaitDuration = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Configures or updates bucket replenishment parameters for the specified key.
    /// </summary>
    void ConfigureBucket(string key, double tokensPerSecond, double maxBurstTokens);
}
