using VK.Blocks.AI.Tokenics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Configuration settings for the Token Limiting and Quota feature.
/// </summary>
[VKFeature(typeof(TokenicsFeature), GenerateValidator = true)]
public sealed partial record VKQuotasOptions : IVKAIQuotaSettings, IVKToggleableBlockOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether Token Limiting is enabled.
    /// Defaults to false.
    /// </summary>
    public bool Enabled { get; init; } = false;

    /// <inheritdoc />
    public long? GlobalTokenLimit { get; init; }

    /// <inheritdoc />
    public long? MonthlyTokenBudget { get; init; }

    /// <inheritdoc />
    public int? RateLimitPerMinute { get; init; }
}
