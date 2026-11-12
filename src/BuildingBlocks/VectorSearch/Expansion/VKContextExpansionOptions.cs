using VK.Blocks.Core;

namespace VK.Blocks.VectorSearch;

/// <summary>
/// Options for the Context Expansion stage. Disabled by default per vector pruning architecture.
/// </summary>
[VKOptions(ArgsMode = VKArgsGenerationMode.Explicit, GenerateMerge = true)]
public sealed partial record VKContextExpansionOptions : IVKToggleableBlockOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether the Context Expansion stage is enabled.
    /// Defaults to false per Phase 2 ablation standards.
    /// </summary>
    public bool Enabled { get; init; } = false;

    /// <summary>
    /// Gets the window size (N) for sliding window context expansion [current-N, current+N].
    /// </summary>
    public int WindowSize { get; init; } = 1;
}
