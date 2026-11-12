using VK.Blocks.Core;

namespace VK.Blocks.VectorSearch;

/// <summary>
/// Options for the Query Rewrite stage. Disabled by default per vector pruning architecture.
/// </summary>
[VKOptions(ArgsMode = VKArgsGenerationMode.Explicit, GenerateMerge = true)]
public sealed partial record VKQueryRewriteOptions : IVKToggleableBlockOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether the Query Rewrite stage is enabled.
    /// Defaults to false per Phase 2 ablation standards.
    /// </summary>
    public bool Enabled { get; init; } = false;
}
