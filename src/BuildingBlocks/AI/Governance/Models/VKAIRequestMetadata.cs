using System.Collections.Generic;

namespace VK.Blocks.AI;

/// <summary>
/// Metadata context containing business tags, user identification, and correlation details for AI requests.
/// </summary>
public sealed record VKAIRequestMetadata
{
    /// <summary>
    /// Gets the unique identifier of the requesting user.
    /// </summary>
    public string? UserId { get; init; }

    /// <summary>
    /// Gets the unique identifier of the session.
    /// </summary>
    public string? SessionId { get; init; }

    /// <summary>
    /// Gets the correlation identifier for tracing.
    /// </summary>
    public string? CorrelationId { get; init; }

    /// <summary>
    /// Gets the name of the requesting application.
    /// </summary>
    public string? ApplicationName { get; init; }

    /// <summary>
    /// Gets any custom metadata tags associated with the request.
    /// </summary>
    public IReadOnlyDictionary<string, string> Tags { get; init; } = new Dictionary<string, string>();
}
