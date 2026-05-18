using System;
using System.Collections.Generic;

using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Cognitive-aware arguments for chat execution.
/// Following AP.05: Hierarchical configuration pattern.
/// </summary>
public sealed record VKCognitiveChatArgs : IVKAIArgs, IVKGenerationOptions, IVKArgs<VKCognitiveChatArgs>
{
    /// <summary>
    /// Gets an empty set of arguments (no overrides).
    /// </summary>
    public static VKCognitiveChatArgs Empty { get; } = new();

    /// <inheritdoc />
    public float? Temperature { get; init; }

    /// <inheritdoc />
    public int? MaxTokens { get; init; }

    /// <inheritdoc />
    public float? TopP { get; init; }

    /// <inheritdoc />
    public IDictionary<string, object> Context { get; init; } = new Dictionary<string, object>();

    /// <inheritdoc />
    public VKAIProviderType? Provider { get; init; }

    /// <inheritdoc />
    public string? ModelId { get; init; }

    /// <inheritdoc />
    public TimeSpan? Timeout { get; init; }

    /// <inheritdoc />
    public string? UserId { get; init; }


    /// <summary>
    /// Gets the persona execution arguments.
    /// </summary>
    public VKPersonaArgs? Persona { get; init; }

}
