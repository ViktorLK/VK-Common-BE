using System;
using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Configuration settings for the Chat feature.
/// </summary>
[VKFeature(typeof(VKAIBlock), GenerateArgs = true, GenerateValidator = true)]
public sealed partial record VKChatOptions : IVKChatOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether Chat feature is enabled.
    /// Defaults to true.
    /// </summary>
    public bool Enabled { get; init; } = true;

    // --- Connection ---

    /// <inheritdoc />
    public VKAIProviderType? Provider { get; init; }

    /// <inheritdoc />
    public string? ModelId { get; init; }

    /// <inheritdoc />
    public VKSensitiveString? ApiKey { get; init; }

    /// <inheritdoc />
    public string? Endpoint { get; init; }
    // --- Resilience ---

    /// <inheritdoc />
    public TimeSpan? Timeout { get; init; }

    /// <inheritdoc />
    public int? RetryCount { get; init; }

    /// <inheritdoc />
    public int? CircuitBreakerThreshold { get; init; }

    /// <inheritdoc />
    public TimeSpan? CircuitBreakerBreakDuration { get; init; }

    // --- Audit ---

    /// <inheritdoc />
    public bool? EnableAudit { get; init; }

    // --- Quota ---

    /// <inheritdoc />
    public long? GlobalTokenLimit { get; init; }

    /// <inheritdoc />
    public long? MonthlyTokenBudget { get; init; }

    /// <inheritdoc />
    public int? RateLimitPerMinute { get; init; }

    // --- Safety ---

    /// <inheritdoc />
    public bool? EnableContentFilter { get; init; }

    /// <summary>
    /// Gets or sets whether to log the fully rendered prompt sent to the LLM.
    /// </summary>
    public bool EnablePromptLogging { get; init; } = false;

    // --- Chat Specific ---

    /// <summary>
    /// Gets or sets the temperature.
    /// </summary>
    public float? Temperature { get; init; } = 0.7f;

    /// <summary>
    /// Gets or sets the top-p sampling value.
    /// </summary>
    public float? TopP { get; init; } = 1.0f;

    /// <summary>
    /// Gets or sets the frequency penalty.
    /// </summary>
    public float? FrequencyPenalty { get; init; } = 0.0f;

    /// <summary>
    /// Gets or sets the presence penalty.
    /// </summary>
    public float? PresencePenalty { get; init; } = 0.0f;

    /// <summary>
    /// Gets or sets the maximum tokens to generate.
    /// </summary>
    public int? MaxTokens { get; init; } = 512;

    /// <summary>
    /// Gets or sets the maximum context window size for the model.
    /// </summary>
    public int ContextWindowSize { get; init; } = 4096;

    /// <summary>
    /// Gets or sets the number of tokens to reserve for the assistant response.
    /// </summary>
    public int ResponseReservedTokens { get; init; } = 512;

    /// <summary>
    /// Gets or sets the number of tokens to reserve for the system prompt.
    /// </summary>
    public int SystemPromptReservedTokens { get; init; } = 512;

    /// <summary>
    /// Gets or sets the stop sequences.
    /// </summary>
    public IReadOnlyList<string>? StopSequences { get; init; } = [];

    /// <summary>
    /// Gets or sets a value indicating whether streaming is enabled.
    /// </summary>
    public bool? StreamingEnabled { get; init; } = true;

    /// <summary>
    /// Gets or sets the default system prompt.
    /// If provided, it will be injected as the first message if no system message exists in history.
    /// </summary>
    public string? DefaultSystemPrompt { get; init; }

    /// <summary>
    /// Gets or sets the maximum number of history messages to retain.
    /// If null, no trimming is performed.
    /// </summary>
    public int? MaxHistoryMessages { get; init; }

    /// <summary>
    /// Gets or sets the tools available for the chat engine.
    /// </summary>
    public IReadOnlyList<IVKAtomicTool>? Tools { get; init; } = [];

    /// <summary>
    /// Gets or sets a value indicating whether Semantic Kernel's automatic function calling
    /// (<see cref="Microsoft.SemanticKernel.FunctionChoiceBehavior.Auto()"/>) is enabled.
    /// When true, the LLM may autonomously invoke registered kernel plugins during a single chat request.
    /// Defaults to <c>false</c> to preserve backward compatibility.
    /// </summary>
    public bool EnableAutoToolCalling { get; init; } = false;

    /// <summary>
    /// Gets or sets the maximum number of automatic tool call rounds the LLM is allowed to perform
    /// within a single <see cref="SendAsync"/> invocation.
    /// Prevents runaway auto-invocation loops.
    /// Defaults to <c>10</c>. Ignored when <see cref="EnableAutoToolCalling"/> is <c>false</c>.
    /// </summary>
    public int MaxAutoToolCallRounds { get; init; } = 10;
}
