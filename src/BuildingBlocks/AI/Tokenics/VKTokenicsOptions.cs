using VK.Blocks.AI.Tokenics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Configuration settings for the Tokenics feature.
/// </summary>
public sealed record VKTokenicsOptions : IVKAIQuotaSettings, IVKBlockOptions
{
    /// <summary>
    /// The configuration section name for Tokenics options.
    /// </summary>
    public static string SectionName => $"{VKAIOptions.SectionName}:{TokenicsConstants.FeatureName}";

    /// <summary>
    /// Gets or sets a value indicating whether Tokenics feature is enabled.
    /// Defaults to true.
    /// </summary>
    public bool Enabled { get; init; } = true;

    // --- Quota ---

    /// <inheritdoc />
    public long? GlobalTokenLimit { get; init; }

    /// <inheritdoc />
    public long? MonthlyTokenBudget { get; init; }

    /// <inheritdoc />
    public int? RateLimitPerMinute { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether to enable metric export for token usage.
    /// Defaults to true.
    /// </summary>
    public bool EnableMetricExport { get; init; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to enable detailed tracking (by model, tenant, user).
    /// Defaults to false.
    /// </summary>
    public bool EnableDetailedTracking { get; init; } = false;
}
