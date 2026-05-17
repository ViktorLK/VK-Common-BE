using System;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Configuration settings for the Tokenics feature Hub.
/// </summary>
[VKFeature(typeof(VKAIBlock))]
public sealed partial record VKTokenicsOptions : IVKToggleableBlockOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether Tokenics feature is enabled.
    /// Defaults to false.
    /// </summary>
    public bool Enabled { get; init; } = false;

    /// <summary>
    /// Gets or sets the execution timeout for tokenics operations.
    /// </summary>
    public TimeSpan? Timeout { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether to enable metric export for token usage.
    /// </summary>
    public bool EnableMetrics { get; init; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to enable detailed audit logging for token usage.
    /// </summary>
    public bool EnableAuditLog { get; init; } = false;
}
