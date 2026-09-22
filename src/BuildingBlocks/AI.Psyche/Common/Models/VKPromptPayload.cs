using System;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Represents the prompt textual payload and precalculated token estimation.
/// Immutable Value Object. Follows AP.01.
/// </summary>
public sealed record VKPromptPayload
{
    private readonly string _content = string.Empty;
    private readonly int _tokenCount = 0;

    /// <summary>
    /// Gets the raw prompt text content.
    /// </summary>
    public string Content
    {
        get => _content;
        init => _content = VKGuard.NotNull(value, nameof(Content));
    }

    /// <summary>
    /// Gets the precalculated or estimated token count for this payload.
    /// Default is 0 (uncalculated).
    /// </summary>
    public int TokenCount
    {
        get => _tokenCount;
        init => _tokenCount = Math.Max(0, value);
    }

    /// <summary>
    /// Creates a new payload from content and optional token count.
    /// </summary>
    public static VKPromptPayload Create(string content, int tokenCount = 0) => new()
    {
        Content = content,
        TokenCount = tokenCount
    };

    /// <summary>
    /// Empty payload instance.
    /// </summary>
    public static VKPromptPayload Empty { get; } = new();
}
