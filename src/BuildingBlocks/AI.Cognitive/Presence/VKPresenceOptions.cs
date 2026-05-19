using System;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Options for the Presence (Sentiment &amp; Emotional Awareness) feature.
/// </summary>
[VKFeature(typeof(VKAICognitiveBlock), GenerateArgs = true, GenerateValidator = true)]
public sealed partial record VKPresenceOptions : IVKPresenceOptions
{
    /// <inheritdoc />
    public bool Enabled { get; init; } = true;

    /// <inheritdoc />
    public float SentimentThreshold { get; init; } = 0.5f;

    /// <inheritdoc />
    public string? Scenario { get; init; } = "Default";

    /// <summary>
    /// Gets a value indicating whether proactive background heartbeat check is enabled.
    /// </summary>
    public bool ProactiveEnabled { get; init; } = false;

    /// <summary>
    /// Gets the interval at which inactivity checks are run.
    /// </summary>
    public TimeSpan CheckInterval { get; init; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets the threshold of inactivity after which proactive pulse is triggered.
    /// </summary>
    public TimeSpan InactivityThreshold { get; init; } = TimeSpan.FromMinutes(5);
}
