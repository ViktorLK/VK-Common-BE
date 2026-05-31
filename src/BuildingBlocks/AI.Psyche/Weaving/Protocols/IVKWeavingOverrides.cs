using System.Collections.Generic;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Defines weaving settings that can be overridden at the request level.
/// </summary>
public interface IVKWeavingOverrides
{
    /// <summary>
    /// Gets the maximum token limit for the weaving process.
    /// </summary>
    int? MaxTokenLimit { get; init; }

    int? TotalContextLimit { get; init; }
    int? MaxResponseTokens { get; init; }
    int? ReservedSystemTokens { get; init; }
    int? AvailableHistoryLimit { get; init; }
    int? AvailableKnowledgeLimit { get; init; }

    /// <summary>
    /// Gets whether to strip think tags (e.g. &lt;think&gt;) from the prompt.
    /// </summary>
    bool? StripThinkTags { get; init; }

    /// <summary>
    /// Gets whether to enable semantic pruning when token limits are exceeded.
    /// </summary>
    bool? EnableSemanticPruning { get; init; }
    List<VKPromptTierType>? DisabledTiers { get; init; }
    List<VKPromptTierType>? TierRenderOrderOverrides { get; init; }
}
