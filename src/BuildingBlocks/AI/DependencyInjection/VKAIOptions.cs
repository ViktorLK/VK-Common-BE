using System;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Options for the core AI building block.
/// </summary>
public sealed record VKAIOptions : IVKBlockOptions
{
    /// <summary>
    /// The configuration section name for AI options.
    /// </summary>
    public static string SectionName => $"{VKBlocksConstants.VKBlocksConfigPrefix}:{VKAIBlock.BlockName}";

    /// <summary>
    /// Gets or sets the provider for all AI features.
    /// Specific feature providers can override this value.
    /// Defaults to OpenAI.
    /// </summary>
    public VKAIProviderType Provider { get; init; } = VKAIProviderType.OpenAI;

    /// <summary>
    /// Gets or sets the retry count for AI operations.
    /// </summary>
    public int RetryCount { get; init; } = 3;

    /// <summary>
    /// Gets or sets a value indicating whether to enable audit logging for AI requests.
    /// </summary>
    public bool EnableAudit { get; init; } = true;

    /// <summary>
    /// Gets or sets the default timeout for AI operations.
    /// </summary>
    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets the default circuit breaker threshold (consecutive failures).
    /// </summary>
    public int CircuitBreakerThreshold { get; init; } = 5;

    /// <summary>
    /// Gets or sets the default duration the circuit should remain open when tripped.
    /// </summary>
    public TimeSpan CircuitBreakerBreakDuration { get; init; } = TimeSpan.FromSeconds(15);
}
