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
public sealed record VKEmbeddingArgs : IVKAIArgs, IVKAIProviderOverrides, IVKArgs<VKEmbeddingArgs>
{
    /// <summary>
    /// Gets an empty set of arguments (no overrides).
    /// </summary>
    public static VKEmbeddingArgs Empty { get; } = new();

    /// <inheritdoc />
    public VKAIProviderType? Provider { get; init; }

    /// <inheritdoc />
    public string? ModelId { get; init; }

    /// <inheritdoc />
    public VKSensitiveString? ApiKey { get; init; }

    /// <inheritdoc />
    public string? Endpoint { get; init; }

    /// <inheritdoc />
    public IDictionary<string, object> Context { get; init; } = new Dictionary<string, object>();

    /// <inheritdoc />
    public TimeSpan? Timeout { get; init; }

    /// <inheritdoc />
    public string? UserId { get; init; }

    /// <summary>
    /// Gets the service identifier for providers that support multiple services (e.g., Semantic Kernel).
    /// </summary>
    public string? ServiceId { get; init; }
}
