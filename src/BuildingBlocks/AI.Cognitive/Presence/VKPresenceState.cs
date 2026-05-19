namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Represents the dynamic, session-level presence and environmental state.
/// Follows AP.01 (Sealed Record) and AP.03.
/// </summary>
public sealed record VKPresenceState
{
    /// <summary>
    /// Gets the unique session identifier.
    /// </summary>
    public required string SessionId { get; init; }

    /// <summary>
    /// Gets the active scenario string representing environmental or situational context.
    /// </summary>
    public string? ActiveScenario { get; init; }

    /// <summary>
    /// Gets the running user sentiment analysis score (range -1.0 to 1.0).
    /// </summary>
    public float UserSentimentScore { get; init; } = 0.0f;

    /// <summary>
    /// Gets the current accumulated stress level (0.0 to 1.0).
    /// </summary>
    public double StressLevel { get; init; } = 0.0;

    /// <summary>
    /// Gets the active stress threshold effect state.
    /// </summary>
    public VKPresenceStressEffect StressEffect { get; init; } = VKPresenceStressEffect.None;
}
