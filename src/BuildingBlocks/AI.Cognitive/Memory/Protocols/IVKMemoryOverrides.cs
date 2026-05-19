namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Defines memory settings that can be overridden at the request level.
/// </summary>
public interface IVKMemoryOverrides
{
    /// <summary>
    /// Gets the default relevance threshold for memory search.
    /// </summary>
    float? DefaultMinScore { get; init; }

    /// <summary>
    /// Gets the token threshold to trigger context summarization.
    /// </summary>
    int? SummaryTriggerTokenThreshold { get; init; }

    /// <summary>
    /// Gets the target token count for memory summaries.
    /// </summary>
    int? SummaryTargetTokens { get; init; }

    /// <summary>
    /// Gets the maximum number of long-term memory entries to inject into context.
    /// </summary>
    int? MaxMemoryEntriesToInject { get; init; }
}
