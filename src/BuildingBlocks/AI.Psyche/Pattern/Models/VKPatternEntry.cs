namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Represents a customizable pattern or prompt injection node in the weaving pipeline.
/// Metaphor: Custom prompt nodes defined by users or application logic.
/// </summary>
public sealed record VKPatternEntry : IVKFragmentMetadata
{
    /// <summary>
    /// Gets the unique identifier for the pattern.
    /// </summary>
    public required VKPatternId Id { get; init; }

    /// <summary>
    /// Gets the layout segment coordinates of the pattern.
    /// </summary>
    public required VKPromptSegment Segment { get; init; }
}
