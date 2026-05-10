using System;
using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Arguments for embedding generation.
/// Following AP.05: Hierarchical configuration pattern.
/// Note: Model and Provider should typically be configured in <see cref="VKEmbeddingOptions"/>
/// to ensure vector compatibility across the data set.
/// </summary>
public sealed record VKEmbeddingArgs : IVKAIArgs, IVKArgs<VKEmbeddingArgs>
{
    /// <summary>
    /// Gets an empty set of arguments (no overrides).
    /// </summary>
    public static VKEmbeddingArgs Empty { get; } = new();

    /// <inheritdoc />
    public IDictionary<string, object> Context { get; init; } = new Dictionary<string, object>();

    /// <inheritdoc />
    public TimeSpan? Timeout { get; init; }
}
