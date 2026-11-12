using System;

namespace VK.Blocks.AI;

/// <summary>
/// Context for an AI request, used for tracking and diagnostics.
/// </summary>
public sealed record VKAIRequestContext
{
    /// <summary>
    /// Gets the trace identifier for the request.
    /// </summary>
    public required string TraceId { get; init; }

    /// <summary>
    /// Gets the tenant identifier for the request.
    /// </summary>
    public string? TenantId { get; init; }

    /// <summary>
    /// Gets the user identifier for the request.
    /// </summary>
    public string? UserId { get; init; }

    /// <summary>
    /// Gets the usage information for this request.
    /// </summary>
    public VKAITokenUsage? Usage { get; init; }

    /// <summary>
    /// Gets the timestamp when the request started.
    /// </summary>
    public required DateTimeOffset StartedAt { get; init; }
}
