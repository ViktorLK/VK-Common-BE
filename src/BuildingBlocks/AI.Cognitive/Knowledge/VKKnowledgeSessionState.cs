namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Tracks the active, session-level memory state (turn tracking) for a knowledge entry.
/// Follows AP.01 (Sealed Record) and AP.03.
/// </summary>
public sealed record VKKnowledgeSessionState
{
    /// <summary>
    /// Gets the session identifier.
    /// </summary>
    public required string SessionId { get; init; }

    /// <summary>
    /// Gets the unique knowledge entry identifier.
    /// </summary>
    public required string KnowledgeId { get; init; }

    /// <summary>
    /// Gets the remaining number of sticky turns this knowledge entry stays active.
    /// </summary>
    public int StickyRemainingTurns { get; init; } = 0;

    /// <summary>
    /// Gets the remaining cooldown turns before this entry can trigger again.
    /// </summary>
    public int CooldownRemainingTurns { get; init; } = 0;

    /// <summary>
    /// Gets the global turn index when this knowledge entry was last triggered.
    /// </summary>
    public int LastTriggeredTurnIndex { get; init; } = -1;
}
