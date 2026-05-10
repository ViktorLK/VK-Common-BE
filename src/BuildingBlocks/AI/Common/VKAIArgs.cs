using System;
using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Common implementation of AI execution arguments.
/// Following AP.05: Hierarchical configuration pattern.
/// </summary>
public sealed record VKAIArgs : IVKAIArgs, IVKAIConnectionSettings, IVKGenerationSettings, IVKArgs<VKAIArgs>
{
    /// <summary>
    /// Gets an empty set of arguments (no overrides).
    /// </summary>
    public static VKAIArgs Empty { get; } = new();

    /// <inheritdoc />
    public float? Temperature { get; init; }

    /// <inheritdoc />
    public int? MaxTokens { get; init; }

    /// <inheritdoc />
    public float? TopP { get; init; }

    /// <inheritdoc />
    public IDictionary<string, object> Context { get; init; } = new Dictionary<string, object>();

    /// <inheritdoc />
    public TimeSpan? Timeout { get; init; }

    /// <inheritdoc />
    public VKAIProviderType? Provider { get; init; }

    /// <inheritdoc />
    public string? ModelId { get; init; }
}
