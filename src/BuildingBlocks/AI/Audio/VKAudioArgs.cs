using System;
using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Arguments for audio execution (Transcription/Speech).
/// Following AP.05: Hierarchical configuration pattern.
/// </summary>
public sealed record VKAudioArgs : IVKAIArgs, IVKAIProviderOverrides, IVKArgs<VKAudioArgs>
{
    /// <summary>
    /// Gets an empty set of arguments (no overrides).
    /// </summary>
    public static VKAudioArgs Empty { get; } = new();

    /// <inheritdoc />
    public VKAIProviderType? Provider { get; init; }

    /// <inheritdoc />
    public string? ModelId { get; init; }

    /// <inheritdoc />
    public VKSensitiveString? ApiKey { get; init; }

    /// <inheritdoc />
    public string? Endpoint { get; init; }

    /// <inheritdoc />
    public TimeSpan? Timeout { get; init; }

    /// <summary>
    /// Gets or sets the voice override (for Speech).
    /// </summary>
    public string? Voice { get; init; }

    /// <summary>
    /// Gets or sets the language override (for Transcription).
    /// </summary>
    public string? Language { get; init; }

    /// <inheritdoc />
    public IDictionary<string, object> Context { get; init; } = new Dictionary<string, object>();

    /// <inheritdoc />
    public string? UserId { get; init; }
}
