using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Controls the global behavior and strict layout constraints of the Weaving Engine.
/// Follows BB.05 (Options pattern with sealed record).
/// Uses <see cref="VKArgsGenerationMode.Implicit"/> so all options are automatically available for request-level override.
/// </summary>
public sealed partial record VKWeavingOptions : IVKBlockOptions
{
    /// <summary>
    /// Gets the optional maximum context token budget.
    /// If null (default), dynamically uses the physical ContextWindowSize from <see cref="IVKModelCatalog"/>.
    /// </summary>
    public int? MaxContextBudget { get; init; } = null;

    /// <summary>
    /// Gets the default reserved token budget allocated for LLM response generation when not specified in args.
    /// Default is 2,048.
    /// </summary>
    public int DefaultResponseReservedTokens { get; init; } = 2048;

    /// <summary>
    /// Gets the list of prompt tiers that should be completely disabled and pruned during weaving.
    /// </summary>
    public List<VKPromptTierType> DisabledTiers { get; init; } = [];

    /// <summary>
    /// Gets the override order sequence for prompt tier rendering.
    /// </summary>
    public List<VKPromptTierType> TierRenderOrderOverrides { get; init; } = [];
}
