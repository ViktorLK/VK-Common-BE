namespace VK.Blocks.AI.Gateway.Internal;

/// <summary>
/// Manages request rates (RPM) and concurrency (active requests) limits for AI providers.
/// </summary>
internal interface IAIRateLimiter
{
    /// <summary>
    /// Checks if a request is allowed under concurrency and rate limits.
    /// </summary>
    bool IsAllowed(IVKAIProviderOptions config);

    /// <summary>
    /// Acquires a slot for in-flight requests and records the request timestamp.
    /// </summary>
    void Acquire(IVKAIProviderOptions config);

    /// <summary>
    /// Releases a concurrency slot for a completed request.
    /// </summary>
    void Release(IVKAIProviderOptions config);
}
