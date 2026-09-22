using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Represents an assembled prompt segment combining layout coordinates and prompt textual payload.
/// Follows AP.01.
/// </summary>
public sealed record VKPromptSegment
{
    /// <summary>
    /// Gets the layout coordinates and placement rules for this segment.
    /// </summary>
    public VKPromptCoordinates Coordinates { get; init; } = new();

    /// <summary>
    /// Gets the prompt text payload and precalculated token count.
    /// </summary>
    public VKPromptPayload Payload { get; init; } = new();

    /// <summary>
    /// Initializes a new instance of <see cref="VKPromptSegment"/>.
    /// </summary>
    public VKPromptSegment() { }

    /// <summary>
    /// Initializes a new instance of <see cref="VKPromptSegment"/> with specified coordinates and payload.
    /// </summary>
    public VKPromptSegment(VKPromptCoordinates coordinates, VKPromptPayload payload)
    {
        Coordinates = VKGuard.NotNull(coordinates);
        Payload = VKGuard.NotNull(payload);
    }
}
