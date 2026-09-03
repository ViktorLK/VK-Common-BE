using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.Resilience;

/// <summary>
/// Represents metadata describing a resilience strategy or pipeline policy.
/// Follows [AP.01], [BB.01], [BB.05].
/// </summary>
public sealed record VKResilienceMetadata
{
    /// <summary>
    /// Gets the unique name of the strategy (e.g. "Retry", "Timeout", "CircuitBreaker").
    /// </summary>
    public required string StrategyName { get; init; }

    /// <summary>
    /// Gets the execution order of the strategy in the pipeline (lower values execute outer).
    /// </summary>
    public int Order { get; init; }

    /// <summary>
    /// Gets the target resource or partition key associated with this policy.
    /// </summary>
    public string? Key { get; init; }

    /// <summary>
    /// Gets a human-readable description of this policy.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets custom metadata properties.
    /// </summary>
    public IReadOnlyDictionary<string, object> Properties { get; init; } = new Dictionary<string, object>();
}
