using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Represents layout segment coordinates (position strategy and rendering priority) along with prompt payload of a prompt segment.
/// </summary>
public sealed record VKPromptSegment
{
    private readonly int _depthPriority = 0;

    /// <summary>
    /// Gets a value indicating whether this prompt fragment is active and enabled.
    /// </summary>
    public bool IsEnabled { get; init; } = true;

    /// <summary>
    /// Gets the prompt text content.
    /// </summary>
    public string Content { get; init; } = string.Empty;

    /// <summary>
    /// Gets the optional name of the prompt entry for identification.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Gets the chat role (e.g., System, User, Assistant) under which this segment is presented.
    /// </summary>
    public VKChatRole Role { get; init; } = VKChatRole.System;

    /// <summary>
    /// Gets the absolute depth (position) in the message layout if absolute positioning is used; otherwise, null.
    /// </summary>
    public int? AbsoluteDepth { get; init; }

    /// <summary>
    /// Gets the relative anchor relative to which the segment is rendered if absolute positioning is not used.
    /// </summary>
    public VKPromptRelativeDepth? RelativeDepth { get; init; }

    /// <summary>
    /// Gets the rendering priority order. Priority must be between 0 and 999.
    /// </summary>
    public int DepthPriority
    {
        get => _depthPriority;
        init => _depthPriority = VKGuard.InRange(value, 0, 999, nameof(DepthPriority));
    }
}
