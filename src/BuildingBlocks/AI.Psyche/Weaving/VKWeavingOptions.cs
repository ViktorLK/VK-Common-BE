using System.Collections.Generic;
using VK.Blocks.Core;

// // [AP.03] Public contract in root namespace carrying VK prefix
namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Controls the global behavior and strict layout constraints of the Weaving Engine.
/// Follows BB.05 (Options pattern with sealed record).
/// </summary>
[VKFeature(typeof(VKAIPsycheBlock), GenerateArgs = true, GenerateValidator = true)]
public sealed partial record VKWeavingOptions : IVKWeavingOptions
{

    public int MaxTokenLimit { get; init; } = 32768;

    public int TotalContextLimit { get; init; } = 32768;
    public int MaxResponseTokens { get; init; } = 2048;
    public int ReservedSystemTokens { get; init; } = 1024;
    public int AvailableHistoryLimit { get; init; } = 16384;
    public int AvailableKnowledgeLimit { get; init; } = 8192;

    public bool StripThinkTags { get; init; } = true;
    public bool EnableSemanticPruning { get; init; } = true;

    public List<VKPromptTierType> DisabledTiers { get; init; } = [];
    public List<VKPromptTierType> TierRenderOrderOverrides { get; init; } = [];
}
