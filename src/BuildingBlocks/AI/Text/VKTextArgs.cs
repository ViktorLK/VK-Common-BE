using System;
using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Text;

/// <summary>
/// Arguments for a text generation request.
/// </summary>
public sealed record VKTextArgs : IVKAIArgs, IVKAIProviderOverrides, IVKGenerationSettings, IVKArgs<VKTextArgs>
{
    /// <summary>
    /// Gets an empty set of arguments (no overrides).
    /// </summary>
    public static VKTextArgs Empty { get; } = new();

    /// <inheritdoc />
    public string? ModelId { get; init; }

    /// <inheritdoc />
    public VKSensitiveString? ApiKey { get; init; }

    /// <inheritdoc />
    public string? Endpoint { get; init; }

    /// <inheritdoc />
    public string? ServiceId { get; init; }

    /// <inheritdoc />
    public VKAIProviderType? Provider { get; init; }

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
    public string? UserId { get; init; }

    /// <summary>
    /// Gets or sets the presence penalty.
    /// </summary>
    public float? PresencePenalty { get; init; }

    /// <summary>
    /// Gets or sets the frequency penalty.
    /// </summary>
    public float? FrequencyPenalty { get; init; }

    /// <summary>
    /// Gets or sets the stop sequences.
    /// </summary>
    public IEnumerable<string>? StopSequences { get; init; }
}
