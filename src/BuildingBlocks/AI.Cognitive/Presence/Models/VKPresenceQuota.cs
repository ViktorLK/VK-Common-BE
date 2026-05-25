namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Represents the dynamic token limits and budgets for a specific session.
/// Follows AP.01 (sealed record with required properties) and AP.03.
/// </summary>
public sealed record VKPresenceQuota
{
    /// <summary>
    /// Gets the maximum token limit for sliding window truncation.
    /// </summary>
    public required int TokenLimit { get; init; }

    /// <summary>
    /// Gets the maximum request-level token quota limit.
    /// </summary>
    public required int MaxRequestTokenQuota { get; init; }

    /// <summary>
    /// Gets the safety token margin buffer before triggering absolute window clipping.
    /// </summary>
    public required int SafetyMarginTokens { get; init; }
}
