using System;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Options for the image generation feature.
/// </summary>
[VKFeature(typeof(VKAIBlock), GenerateArgs = true)]
public sealed partial record VKImageGenerationOptions : IVKImageGenerationOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether the image generation feature is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;

    // --- Connection ---
    public VKAIProviderType? Provider { get; init; }
    public string? ModelId { get; init; }
    public VKSensitiveString? ApiKey { get; init; }
    public string? Endpoint { get; init; }

    // --- Resilience ---
    public TimeSpan? Timeout { get; init; }
    public int? RetryCount { get; init; }
    public int? CircuitBreakerThreshold { get; init; }
    public TimeSpan? CircuitBreakerBreakDuration { get; init; }

    // --- Audit ---
    public bool? EnableAudit { get; init; }

    // --- Quota ---
    public long? GlobalTokenLimit { get; init; }
    public long? MonthlyTokenBudget { get; init; }
    public int? RateLimitPerMinute { get; init; }

    // --- Safety ---
    public bool? EnableContentFilter { get; init; }

    // --- Specific Settings ---
    public int? Width { get; init; } = 1024;
    public int? Height { get; init; } = 1024;
    public string? AspectRatio { get; init; }
}
