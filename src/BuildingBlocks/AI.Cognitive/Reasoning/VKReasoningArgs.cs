using System;
using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Arguments for goal reasoning.
/// Following AP.05: Hierarchical configuration pattern.
/// </summary>
public sealed record VKReasoningArgs : IVKAIArgs, IVKArgs<VKReasoningArgs>
{
    /// <summary>
    /// Gets an empty set of arguments (no overrides).
    /// </summary>
    public static VKReasoningArgs Empty { get; } = new();

    /// <inheritdoc />
    public string? ModelName { get; init; }

    /// <inheritdoc />
    public IDictionary<string, object> Context { get; init; } = new Dictionary<string, object>();

    /// <inheritdoc />
    public TimeSpan? Timeout { get; init; }

    /// <inheritdoc />
    public string? UserId { get; init; }

    /// <summary>
    /// Gets a value indicating whether to allow parallel execution of independent steps.
    /// </summary>
    public bool AllowParallelism { get; init; } = true;

    /// <summary>
    /// Gets the maximum depth for hierarchical reasoning.
    /// </summary>
    public int MaxDepth { get; init; } = 3;
}
