namespace VK.Blocks.AI;

/// <summary>
/// Categorizes the failure mode of an AI provider invocation.
/// </summary>
public enum VKAIFailureKind : byte
{
    /// <summary>
    /// No failure occurred.
    /// </summary>
    None = 0,

    /// <summary>
    /// A network or transport-level failure occurred.
    /// </summary>
    Transport = 1,

    /// <summary>
    /// The remote AI provider returned a server error.
    /// </summary>
    ServerError = 2,

    /// <summary>
    /// The request was rate-limited by the provider.
    /// </summary>
    RateLimited = 3,

    /// <summary>
    /// Authentication or authorization with the provider failed.
    /// </summary>
    AuthFailed = 4,

    /// <summary>
    /// The request payload was rejected as invalid.
    /// </summary>
    BadRequest = 5,

    /// <summary>
    /// The content was rejected by safety or moderation filters.
    /// </summary>
    ContentRejected = 6,

    /// <summary>
    /// The invocation was cancelled.
    /// </summary>
    Cancelled = 7,

    /// <summary>
    /// The invocation timed out before completion.
    /// </summary>
    Timeout = 8,

    /// <summary>
    /// The streaming response was interrupted after initial output was committed.
    /// </summary>
    StreamInterrupted = 9
}
