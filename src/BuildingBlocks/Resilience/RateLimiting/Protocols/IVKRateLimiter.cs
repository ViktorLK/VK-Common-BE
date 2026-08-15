using System;

namespace VK.Blocks.Resilience;

/// <summary>
/// Defines the contract for rate limiting and throughput control.
/// </summary>
public interface IVKRateLimiter
{
    /// <summary>
    /// Checks if a request is allowed within the rate limit and window.
    /// </summary>
    bool IsAllowed(string key, int permitLimit, TimeSpan? window = null);

    /// <summary>
    /// Records a request timestamp for the specified key.
    /// </summary>
    void RecordRequest(string key);
}
