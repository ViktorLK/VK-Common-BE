using System.Collections.Generic;

// // [AP.03] Public contract in root namespace carrying VK prefix
namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Holds state context for the active cognitive weaving pipeline execution.
/// Follows AP.01 (Sealed Class with required properties and immutable inputs).
/// </summary>
public sealed class VKWeavingContext
{
    // Input (defined on creation, immutable thereafter)
    public required VKCognitivePipelineContext Pipeline { get; init; }
    public required VKTokenBudgetPlan Budget { get; init; }
    public required VKIntent Intent { get; init; }

    // Stage-by-stage output (progressively populated)
    public IReadOnlyList<VKPromptFragment>? Fragments { get; internal set; }
    public IReadOnlyList<VKScoredFragment>? Scored { get; internal set; }
    public IReadOnlyList<VKScoredFragment>? Pruned { get; internal set; }
    public IReadOnlyList<VKScoredFragment>? Truncated { get; internal set; }
    public IReadOnlyList<VKFormattedTier>? Formatted { get; internal set; }
    public VKPromptTapestry? Tapestry { get; internal set; }
}
