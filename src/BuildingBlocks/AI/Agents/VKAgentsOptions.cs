using System;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Configuration settings for the Agents feature.
/// </summary>
[VKFeature(typeof(VKAIBlock), GenerateArgs = true, GenerateValidator = true)]
public sealed partial record VKAgentsOptions : IVKAgentsOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether Agents feature is enabled.
    /// Defaults to false.
    /// </summary>
    public bool Enabled { get; init; } = false;

    // --- Connection ---
    public VKAIProviderType? Provider { get; init; }
    public string? ModelId { get; init; }
    public VKSensitiveString? ApiKey { get; init; }
    public string? Endpoint { get; init; }

    // --- Resilience ---
    public TimeSpan? Timeout { get; init; }
    public int? RetryCount { get; init; }
    public int? CircuitBreakerThreshold { get; init; }
    public TimeSpan? CircuitBreakerBreakDuration { get; init; }

    // --- Audit ---
    public bool? EnableAudit { get; init; }

    // --- Quota ---
    public long? GlobalTokenLimit { get; init; }
    public long? MonthlyTokenBudget { get; init; }
    public int? RateLimitPerMinute { get; init; }

    // --- Safety ---
    public bool? EnableContentFilter { get; init; }

    /// <summary>
    /// Gets or sets the maximum number of iterations for an agent.
    /// </summary>
    public int? MaxIterations { get; init; } = 10;

    /// <summary>
    /// Gets or sets the maximum number of tool calls per iteration.
    /// </summary>
    public int MaxToolCallsPerIteration { get; init; } = 5;

    /// <summary>
    /// Gets or sets the maximum total tokens allowed for the entire agentic run.
    /// Optional.
    /// </summary>
    public int? MaxTotalTokens { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether to allow parallel tool calls.
    /// Defaults to true.
    /// </summary>
    public bool AllowParallelToolCalls { get; init; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to log detailed tool arguments and results.
    /// **WARNING**: This may include PII. Defaults to false.
    /// </summary>
    public bool LogToolData { get; init; } = false;

    /// <summary>
    /// Gets or sets the number of retries for tool execution failures.
    /// Optional. If not set, falls back to <see cref="VKAIOptions.RetryCount"/>.
    /// </summary>
    public int? ToolRetryCount { get; init; }

    /// <summary>
    /// Gets or sets the delay in milliseconds between tool retries.
    /// Defaults to 500ms.
    /// </summary>
    public int ToolRetryBackoffMs { get; init; } = 500;

    /// <summary>
    /// Gets or sets the maximum number of history messages to retain.
    /// If null, no trimming is performed.
    /// </summary>
    public int? MaxHistoryMessages { get; init; }

    /// <summary>
    /// Gets or sets the maximum length of tool result content.
    /// Truncates results exceeding this limit to prevent token explosion.
    /// Defaults to 4000.
    /// </summary>
    public int? MaxToolResultLength { get; init; } = 4000;

    /// <summary>
    /// Gets or sets the default system prompt for the agent.
    /// If provided, it will be injected as the first message if no system message exists.
    /// </summary>
    public string? DefaultSystemPrompt { get; init; }

    /// <summary>
    /// Gets or sets the chat settings for the agent.
    /// Following AP.05: Nested hierarchical configuration.
    /// </summary>
    public VKChatOptions Chat { get; init; } = new();
}
