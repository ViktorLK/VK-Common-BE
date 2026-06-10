using VK.Blocks.Core;

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
    /// Gets the name of the pattern.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the content of the pattern prompt.
    /// </summary>
    public required string Content { get; init; }

    /// <summary>
    /// Gets the target prompt template position where this pattern should be woven.
    /// Defaults to <see cref="VKRelativePromptPosition"/> at <see cref="VKPromptRelativeAnchor.AfterPersona"/>.
    /// </summary>
    public IVKPromptPosition Position { get; init; } = new VKRelativePromptPosition(VKPromptRelativeAnchor.AfterPersona);

    private readonly int _priority = 0;

    /// <summary>
    /// Gets the rendering priority order of the pattern.
    /// Priority must be between 0 and 999.
    /// </summary>
    public int Priority
    {
        get => _priority;
        init => _priority = VKGuard.InRange(value, 0, 999, nameof(Priority));
    }
}
