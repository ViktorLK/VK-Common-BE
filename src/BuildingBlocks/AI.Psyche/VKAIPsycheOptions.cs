using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Top-level configuration options for the AIPsyche building block.
/// Governs global token budget bounds, fallback policies, and pipeline defaults.
/// Follows BB.05 (Options pattern with sealed partial record).
/// </summary>
public sealed partial record VKAIPsycheOptions : IVKBlockOptions
{
    /// <summary>
    /// Gets the optional global maximum context token budget for all pipeline executions.
    /// If null (default), falls back to the resolved AI model's physical ContextWindowSize.
    /// </summary>
    public int? MaxContextBudget { get; init; }

    /// <summary>
    /// Gets the default token budget reserved for LLM response completion.
    /// Defaults to 4096 tokens.
    /// </summary>
    public int ResponseReservedTokens { get; init; } = 4096;
}
