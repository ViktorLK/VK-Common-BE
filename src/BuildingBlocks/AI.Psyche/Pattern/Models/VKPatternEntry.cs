namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Represents a custom prompt preset pattern woven into the prompt tapestry.
/// Metaphor: Custom prompt nodes defined by users/tenants.
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
