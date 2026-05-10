namespace VK.Blocks.AI;

/// <summary>
/// Provides a base implementation for AI quota settings.
/// </summary>
public sealed record VKAIQuotaSettings : IVKAIQuotaSettings
{
    /// <inheritdoc />
    public long? GlobalTokenLimit { get; init; }

    /// <inheritdoc />
    public long? MonthlyTokenBudget { get; init; }

    /// <inheritdoc />
    public int? RateLimitPerMinute { get; init; }
}
