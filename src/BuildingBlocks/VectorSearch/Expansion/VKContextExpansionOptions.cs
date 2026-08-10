using VK.Blocks.Core;

namespace VK.Blocks.VectorSearch;

/// <summary>
/// Options for the Context Expansion stage.
/// </summary>

public sealed partial record VKContextExpansionOptions : IVKToggleableBlockOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether the Context Expansion stage is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets the window size (N) for sliding window context expansion [current-N, current+N].
    /// </summary>
    public int WindowSize { get; init; } = 1;
}
