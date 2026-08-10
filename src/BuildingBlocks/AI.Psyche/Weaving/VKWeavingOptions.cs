using System.Collections.Generic;
using VK.Blocks.Core;

// // [AP.03] Public contract in root namespace carrying VK prefix
namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Controls the global behavior and strict layout constraints of the Weaving Engine.
/// Follows BB.05 (Options pattern with sealed record).
/// Uses <see cref="VKArgsGenerationMode.Implicit"/> so all options are automatically available for request-level override.
/// </summary>
public sealed partial record VKWeavingOptions : IVKBlockOptions
{
    /// <summary>
    /// Gets the absolute maximum token limit supported by the downstream LLM model. Default is 32,768.
    /// </summary>
    public int MaxTokenLimit { get; init; } = 32768;

    /// <summary>
    /// Gets the total allowable context window token limit for prompt assembly. Default is 32,768.
    /// </summary>
    public int TotalContextLimit { get; init; } = 32768;

    /// <summary>
    /// Gets the reserved token budget allocated for LLM response generation. Default is 2,048.
    /// </summary>
    public int MaxResponseTokens { get; init; } = 2048;

    /// <summary>
    /// Gets the maximum allowable token budget for dialogue history (Echo fragments). Default is 16,384.
    /// </summary>
    public int AvailableHistoryLimit { get; init; } = 16384;

    /// <summary>
    /// Gets the maximum allowable token budget for injected knowledge fragments. Default is 8,192.
    /// </summary>
    public int AvailableKnowledgeLimit { get; init; } = 8192;

    /// <summary>
    /// Gets the list of prompt tiers that should be completely disabled and pruned during weaving.
    /// </summary>
    public List<VKPromptTierType> DisabledTiers { get; init; } = [];

    /// <summary>
    /// Gets the override order sequence for prompt tier rendering.
    /// </summary>
    public List<VKPromptTierType> TierRenderOrderOverrides { get; init; } = [];
}
