using System;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Root infrastructure configuration for the AI building block.
/// Governs sensitive logging compliance, turn deadline, and resilience fallback configuration.
/// Feature-specific settings (Chat, Embeddings) belong to their respective feature options.
/// </summary>
[VKOptions]
public sealed partial record VKAIOptions : IVKBlockOptions
{
    /// <summary>
    /// Gets or sets whether sensitive user prompts and completions can be emitted to telemetry/logs.
    /// Defaults to false for privacy &amp; GDPR compliance.
    /// </summary>
    public bool EnableSensitiveDataLogging { get; init; } = false;

    /// <summary>
    /// Gets or sets the logical turn deadline applied to AI orchestration turns.
    /// Defaults to <see cref="VKAIConstants.DefaultTurnDeadline"/>.
    /// </summary>
    public TimeSpan TurnDeadline { get; init; } = VKAIConstants.DefaultTurnDeadline;

    /// <summary>
    /// Gets or sets a value indicating whether a no-op resilience executor is permitted at startup.
    /// Defaults to false so missing resilience wiring fails fast per [AP.06].
    /// </summary>
    public bool AllowNoOpResilience { get; init; } = false;
}
