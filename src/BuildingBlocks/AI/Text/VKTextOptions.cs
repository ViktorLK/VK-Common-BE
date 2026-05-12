using VK.Blocks.AI;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Text;

/// <summary>
/// Options for the text generation feature.
/// </summary>
public sealed record VKTextOptions : IVKToggleableBlockOptions, IVKAIProviderSettings, IVKAIGovernanceSettings
{
    /// <inheritdoc />
    public static string SectionName => $"{VKAIOptions.SectionName}:Text";

    /// <inheritdoc />
    public bool Enabled { get; init; } = true;

    /// <inheritdoc />
    public string? ModelId { get; init; }

    /// <inheritdoc />
    public VKSensitiveString? ApiKey { get; init; }

    /// <inheritdoc />
    public string? Endpoint { get; init; }

    /// <inheritdoc />
    public string? ServiceId { get; init; }

    /// <inheritdoc />
    public VKAIProviderType? Provider { get; init; }

    /// <inheritdoc />
    public int? RetryCount { get; init; }

    /// <inheritdoc />
    public System.TimeSpan? Timeout { get; init; }

    /// <inheritdoc />
    public int? CircuitBreakerThreshold { get; init; }

    /// <inheritdoc />
    public System.TimeSpan? CircuitBreakerBreakDuration { get; init; }

    // --- Audit ---

    /// <inheritdoc />
    public bool? EnableAudit { get; init; }

    // --- Quota ---

    /// <inheritdoc />
    public long? GlobalTokenLimit { get; init; }

    /// <inheritdoc />
    public long? MonthlyTokenBudget { get; init; }

    /// <inheritdoc />
    public int? RateLimitPerMinute { get; init; }

    // --- Safety ---

    /// <inheritdoc />
    public bool? EnableContentFilter { get; init; }

    // --- Text Specific ---

    /// <summary>
    /// Gets or sets the default temperature for text generation.
    /// </summary>
    public float? Temperature { get; init; } = 0.7f;

    /// <summary>
    /// Gets or sets the default maximum number of tokens for text generation.
    /// </summary>
    public int? MaxTokens { get; init; } = 512;
}
