namespace VK.Blocks.AI;

/// <summary>
/// Defines quota and cost parameters for AI features.
/// </summary>
public interface IVKAIQuotaSettings
{
    /// <summary>
    /// Gets the global token limit for this specific feature.
    /// Set to 0 or null for no limit.
    /// </summary>
    long? GlobalTokenLimit { get; init; }

    /// <summary>
    /// Gets the monthly token budget for this specific feature.
    /// </summary>
    long? MonthlyTokenBudget { get; init; }

    /// <summary>
    /// Gets the maximum requests allowed per minute for this specific feature.
    /// </summary>
    int? RateLimitPerMinute { get; init; }
}
