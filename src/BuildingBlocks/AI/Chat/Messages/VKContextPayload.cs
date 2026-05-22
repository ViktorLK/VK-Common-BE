using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Represents a structured payload for AI chat completions with advanced context caching support.
/// Provides safe multi-tenant security boundaries and sliding-window cache correlation keys.
/// Follows AP.01 (sealed record, required fields) and AP.03.
/// </summary>
public sealed record VKContextPayload
{
    /// <summary>
    /// Gets the conversation history.
    /// </summary>
    public required IReadOnlyList<VKChatMessage> Messages { get; init; }

    /// <summary>
    /// Gets a value indicating whether prompt context caching is enabled for this request.
    /// Defaults to <c>true</c>.
    /// </summary>
    public bool EnableContextCaching { get; init; } = true;

    /// <summary>
    /// Gets the unique hash fingerprint for multi-tenant and sliding window cache recognition.
    /// Excludes reflection-based cache lookup penalties.
    /// </summary>
    public required string ContextCacheKey { get; init; }
}
