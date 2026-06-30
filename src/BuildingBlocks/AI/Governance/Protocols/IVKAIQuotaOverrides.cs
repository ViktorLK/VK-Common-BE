namespace VK.Blocks.AI;

/// <summary>
/// Defines quota parameters that can be overridden at the request level.
/// [AP.05] Excludes financial-level configurations like monthly budgets.
/// </summary>
public interface IVKAIQuotaOverrides
{
    /// <summary>
    /// Gets the token limit for this specific request.
    /// </summary>
    long? GlobalTokenLimit { get; init; }

    /// <summary>
    /// Gets the maximum requests allowed per minute for this specific request (override).
    /// </summary>
    int? RateLimitPerMinute { get; init; }
}
