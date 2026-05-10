using System;
using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Arguments for moderation execution.
/// Following AP.05: Hierarchical configuration pattern.
/// </summary>
public sealed record VKModerationArgs : IVKAIArgs, IVKAIConnectionSettings, IVKArgs<VKModerationArgs>
{
    /// <summary>
    /// Gets an empty set of arguments (no overrides).
    /// </summary>
    public static VKModerationArgs Empty { get; } = new();

    /// <inheritdoc />
    public VKAIProviderType? Provider { get; init; }

    /// <inheritdoc />
    public string? ModelId { get; init; }

    /// <inheritdoc />
    public TimeSpan? Timeout { get; init; }

    /// <summary>
    /// Gets or sets the sensitivity threshold override.
    /// </summary>
    public float? SensitivityThreshold { get; init; }

    /// <inheritdoc />
    public IDictionary<string, object> Context { get; init; } = new Dictionary<string, object>();
}
