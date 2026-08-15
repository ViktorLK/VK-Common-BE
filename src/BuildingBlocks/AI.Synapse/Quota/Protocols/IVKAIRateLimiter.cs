namespace VK.Blocks.AI.Synapse;

/// <summary>
/// Manages request rates (RPM) and concurrency (active requests) limits for AI connections.
/// </summary>
public interface IVKAIRateLimiter
{
    /// <summary>
    /// Checks if a request is allowed under concurrency and rate limits.
    /// </summary>
    bool IsAllowed(VKAIConnection connection);

    /// <summary>
    /// Acquires a slot for in-flight requests and records the request timestamp.
    /// </summary>
    void Acquire(VKAIConnection connection);

    /// <summary>
    /// Releases a concurrency slot for a completed request.
    /// </summary>
    void Release(VKAIConnection connection);
}
