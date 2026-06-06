using System;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Default configuration settings for the AI building block.
/// These values serve as fallbacks for all AI features.
/// Following BB.06: Modular Feature Pattern.
/// </summary>
[VKFeature(typeof(VKAIBlock), GenerateValidator = true, Namespace = "VK.Blocks.AI.Common.DependencyInjection")]
public sealed partial record VKAIDefaultsOptions : IVKBlockOptions
{
    /// <summary>
    /// Gets or sets the default provider for all AI features.
    /// Specific feature providers can override this value.
    /// Defaults to OpenAI.
    /// </summary>
    public VKAIProviderType Provider { get; init; } = VKAIProviderType.OpenAI;

    /// <summary>
    /// Gets or sets the global default retry count for AI operations.
    /// </summary>
    public int RetryCount { get; init; } = 3;

    /// <summary>
    /// Gets or sets a value indicating whether to enable global audit logging.
    /// </summary>
    public bool EnableAudit { get; init; } = true;

    /// <summary>
    /// Gets or sets the global default timeout for AI operations.
    /// </summary>
    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(100);

    /// <summary>
    /// Gets or sets the global default circuit breaker threshold (consecutive failures).
    /// </summary>
    public int CircuitBreakerThreshold { get; init; } = 5;

    /// <summary>
    /// Gets or sets the global default duration the circuit should remain open when tripped.
    /// </summary>
    public TimeSpan CircuitBreakerBreakDuration { get; init; } = TimeSpan.FromSeconds(15);
}
