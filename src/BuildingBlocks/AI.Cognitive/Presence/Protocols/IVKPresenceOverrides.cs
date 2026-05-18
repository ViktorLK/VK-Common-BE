namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Request-level overrides contract for the Presence feature.
/// </summary>
public interface IVKPresenceOverrides
{
    /// <summary>
    /// Gets the overridden sentiment threshold.
    /// </summary>
    float? SentimentThreshold { get; init; }

    /// <summary>
    /// Gets the overridden active scenario string defining environmental context.
    /// </summary>
    string? Scenario { get; init; }
}
