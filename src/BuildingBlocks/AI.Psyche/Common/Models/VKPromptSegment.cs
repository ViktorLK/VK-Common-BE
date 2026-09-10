using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Represents layout segment coordinates (position strategy and rendering priority) along with prompt payload of a prompt segment.
/// </summary>
public sealed record VKPromptSegment
{
    private readonly int _depthPriority = 0;

    /// <summary>
    /// Gets the prompt tier classification for this segment.
    /// Internal to VK.Blocks.AI.Psyche; external consumers always default to Dynamic and cannot impersonate core tiers.
    /// </summary>
    internal VKPromptTierType Tier { get; init; } = VKPromptTierType.Dynamic;

    /// <summary>
    /// Gets the prompt text content.
    /// </summary>
    public string Content { get; init; } = string.Empty;

    /// <summary>
    /// Gets the optional XML tag name used to enclose this segment in prompt output.
    /// Contiguous segments sharing the same TagName are automatically coalesced into a single XML root element during weaving.
    /// If null, the content is presented as raw text without XML wrapping.
    /// </summary>
    public string? TagName { get; init; }

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

    /// <summary>
    /// Gets the estimated or precalculated token count for this segment content.
    /// Default is 0 (uncalculated).
    /// </summary>
    public int TokenCount { get; init; } = 0;
}
