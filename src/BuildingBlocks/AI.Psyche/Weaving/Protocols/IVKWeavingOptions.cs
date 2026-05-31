using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Aggregates all static Weaving configuration options.
/// </summary>
public interface IVKWeavingOptions : IVKBlockOptions
{
    /// <summary>
    /// Gets the maximum token limit for the weaving process.
    /// </summary>
    int MaxTokenLimit { get; }

    int TotalContextLimit { get; }
    int MaxResponseTokens { get; }
    int ReservedSystemTokens { get; }
    int AvailableHistoryLimit { get; }
    int AvailableKnowledgeLimit { get; }

    /// <summary>
    /// Gets whether to strip think tags (e.g. &lt;think&gt;) from the prompt.
    /// </summary>
    bool StripThinkTags { get; }

    /// <summary>
    /// Gets whether to enable semantic pruning when token limits are exceeded.
    /// </summary>
    bool EnableSemanticPruning { get; }

    List<VKPromptTierType> DisabledTiers { get; init; }
    List<VKPromptTierType> TierRenderOrderOverrides { get; init; }
}
