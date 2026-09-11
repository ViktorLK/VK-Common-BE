namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Represents a single conversational turn in the dialogue history.
/// Follows AP.01.
/// </summary>
public sealed record VKEchoFragment
{
    /// <summary>
    /// Gets the message content of the dialogue turn.
    /// </summary>
    public required string Content { get; init; }

    /// <summary>
    /// Gets the chat role (User, Assistant, System) for this dialogue turn.
    /// </summary>
    public required VKChatRole Role { get; init; }

    /// <summary>
    /// Gets the temporal turn index (0 = oldest turn, increasing sequentially).
    /// </summary>
    public int TurnIndex { get; init; } = 0;

    /// <summary>
    /// Gets the precalculated token count for this echo dialogue turn.
    /// Default is 0 (uncalculated).
    /// </summary>
    public int TokenCount { get; init; } = 0;
}
