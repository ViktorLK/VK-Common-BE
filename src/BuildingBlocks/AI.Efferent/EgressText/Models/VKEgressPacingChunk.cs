namespace VK.Blocks.AI.Efferent;

/// <summary>
/// Represents a text chunk with calculated human-like pause delay for streaming or UI rendering.
/// </summary>
public sealed record VKEgressPacingChunk
{
    /// <summary>
    /// Gets the text segment content.
    /// </summary>
    public required string Text { get; init; }

    /// <summary>
    /// Gets the calculated pause delay in milliseconds before or after displaying this chunk.
    /// </summary>
    public required int DelayMs { get; init; }

    /// <summary>
    /// Gets the 0-based sequence index of this chunk.
    /// </summary>
    public required int SequenceIndex { get; init; }

    /// <summary>
    /// Gets a value indicating whether this chunk is the final chunk in the sequence.
    /// </summary>
    public required bool IsFinal { get; init; }
}
