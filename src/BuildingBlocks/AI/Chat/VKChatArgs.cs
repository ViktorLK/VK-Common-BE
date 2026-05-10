using System;
using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Arguments for chat execution.
/// Following AP.05: Hierarchical configuration pattern.
/// </summary>
public sealed record VKChatArgs : IVKAIArgs, IVKAIConnectionSettings, IVKGenerationSettings, IVKArgs<VKChatArgs>
{
    /// <summary>
    /// Gets an empty set of arguments (no overrides).
    /// </summary>
    public static VKChatArgs Empty { get; } = new();

    /// <inheritdoc />
    public VKAIProviderType? Provider { get; init; }

    /// <inheritdoc />
    public float? Temperature { get; init; }

    /// <inheritdoc />
    public int? MaxTokens { get; init; }

    /// <inheritdoc />
    public float? TopP { get; init; }

    /// <inheritdoc />
    public string? ModelId { get; init; }

    /// <inheritdoc />
    public TimeSpan? Timeout { get; init; }

    /// <summary>
    /// Gets the service identifier for providers that support multiple services (e.g., Semantic Kernel).
    /// </summary>
    public string? ServiceId { get; init; }

    /// <inheritdoc />
    public IDictionary<string, object> Context { get; init; } = new Dictionary<string, object>();
}
