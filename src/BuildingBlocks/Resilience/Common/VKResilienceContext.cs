using System.Collections.Generic;

namespace VK.Blocks.Resilience;

/// <summary>
/// Represents the execution context for a resilience operation.
/// </summary>
public sealed record VKResilienceContext
{
    /// <summary>
    /// Gets the unique operation key.
    /// </summary>
    public required string OperationKey { get; init; }

    /// <summary>
    /// Gets a dictionary of custom properties for the context.
    /// </summary>
    public IReadOnlyDictionary<string, object> Properties { get; init; } = new Dictionary<string, object>();
}
