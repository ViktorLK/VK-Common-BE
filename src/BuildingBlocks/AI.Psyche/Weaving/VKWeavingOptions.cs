using System.Collections.Generic;
using VK.Blocks.Core;

// // [AP.03] Public contract in root namespace carrying VK prefix
namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Controls the global behavior and strict layout constraints of the Weaving Engine.
/// Follows BB.05 (Options pattern with sealed record).
/// </summary>

public sealed partial record VKWeavingOptions : IVKBlockOptions
{

    [VKRequestOverride]
    public int MaxTokenLimit { get; init; } = 32768;

    [VKRequestOverride]
    public int TotalContextLimit { get; init; } = 32768;
    [VKRequestOverride]
    public int MaxResponseTokens { get; init; } = 2048;
    public int ReservedSystemTokens { get; init; } = 1024;
    [VKRequestOverride]
    public int AvailableHistoryLimit { get; init; } = 16384;
    public int AvailableKnowledgeLimit { get; init; } = 8192;

    [VKRequestOverride]
    public bool StripThinkTags { get; init; } = true;
    public bool EnableSemanticPruning { get; init; } = true;
    [VKRequestOverride]
    public bool WeaveOnly { get; init; } = false;

    [VKRequestOverride]
    public List<VKPromptTierType> DisabledTiers { get; init; } = [];
    [VKRequestOverride]
    public List<VKPromptTierType> TierRenderOrderOverrides { get; init; } = [];
    [VKRequestOverride]
    public IDictionary<string, object?> Variables { get; init; } = new Dictionary<string, object?>();
}
