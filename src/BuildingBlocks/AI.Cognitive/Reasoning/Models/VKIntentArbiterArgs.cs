using System;
using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Arguments for intent arbitration.
/// Following AP.05: Hierarchical configuration pattern.
/// </summary>
public sealed record VKIntentArbiterArgs : IVKAIArgs, IVKArgs<VKIntentArbiterArgs>
{
    /// <summary>
    /// Gets an empty set of arguments (no overrides).
    /// </summary>
    public static VKIntentArbiterArgs Empty { get; } = new();

    /// <inheritdoc />
    public string? ModelName { get; init; }

    /// <inheritdoc />
    public IDictionary<string, object> Context { get; init; } = new Dictionary<string, object>();

    /// <inheritdoc />
    public TimeSpan? Timeout { get; init; }

    /// <inheritdoc />
    public string? UserId { get; init; }

    /// <summary>
    /// Gets a value indicating whether to allow multiple active intents (non-exclusive).
    /// </summary>
    public bool AllowMultipleIntents { get; init; } = false;

    /// <summary>
    /// Gets the minimum confidence threshold for an intent to be considered.
    /// </summary>
    public float ConfidenceThreshold { get; init; } = 0.6f;
}
