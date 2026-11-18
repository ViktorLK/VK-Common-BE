namespace VK.Blocks.AI;

/// <summary>
/// Represents a request to resolve candidate AI connections for an invocation.
/// </summary>
// [AP.01]
public sealed record VKAIRouteRequest
{
    /// <summary>
    /// Gets the logical routing profile name.
    /// </summary>
    public required string Profile { get; init; }

    /// <summary>
    /// Gets a value indicating whether the invocation requires tool-calling support.
    /// </summary>
    public bool RequiresTools { get; init; }

    /// <summary>
    /// Gets a value indicating whether the invocation requires structured output support.
    /// </summary>
    public bool RequiresStructuredOutput { get; init; }

    /// <summary>
    /// Gets the estimated context token count for the request.
    /// </summary>
    public int? EstimatedContextTokens { get; init; }

    /// <summary>
    /// Gets an optional preferred connection identifier.
    /// </summary>
    public string? PreferredConnectionId { get; init; }

    /// <summary>
    /// Gets an optional affinity key for sticky routing.
    /// </summary>
    public string? AffinityKey { get; init; }

    /// <summary>
    /// Gets an optional scope key for tenant or workload isolation.
    /// </summary>
    public VKAIScopeKey? ScopeKey { get; init; }

    /// <summary>
    /// Gets optional correlation tags for observability.
    /// </summary>
    public VKAICorrelationTags? CorrelationTags { get; init; }
}
