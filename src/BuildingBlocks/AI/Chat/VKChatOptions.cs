using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Configuration settings for the Chat feature.
/// Represents baseline direct execution options and generation sampling defaults.
/// Follows BB.05 (Options pattern with sealed record) and AP.05 (SG-driven Args generation).
/// Governance, routing, and connection fallbacks belong to AI.Synapse.
/// Mind prompt assembly and persona orchestration belong to AI.Psyche.
/// </summary>
public sealed partial record VKChatOptions : IVKToggleableBlockOptions, IVKAIProviderOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether Chat feature is enabled.
    /// Defaults to true.
    /// </summary>
    [VKNoRequestOverride]
    public bool Enabled { get; init; } = true;

    // --- 1. Direct Connection Defaults ---

    /// <inheritdoc />
    public VKAIProviderType? Provider { get; init; }

    /// <inheritdoc />
    public string? ModelId { get; init; }

    /// <inheritdoc />
    public VKSensitiveString? ApiKey { get; init; }

    /// <inheritdoc />
    public string? Endpoint { get; init; }

    // --- 2. Sampling Defaults (7 Essentials - 100% matches VKGenerationOptions) ---

    /// <summary>
    /// Gets or sets the temperature for sampling randomness.
    /// </summary>
    public float? Temperature { get; init; } = 0.7f;

    /// <summary>
    /// Gets or sets the top-p sampling value.
    /// </summary>
    public float? TopP { get; init; } = 1.0f;

    /// <summary>
    /// Gets or sets the top-k sampling value (if supported by provider).
    /// </summary>
    public int? TopK { get; init; }

    /// <summary>
    /// Gets or sets the frequency penalty.
    /// </summary>
    public float? FrequencyPenalty { get; init; } = 0.0f;

    /// <summary>
    /// Gets or sets the presence penalty.
    /// </summary>
    public float? PresencePenalty { get; init; } = 0.0f;

    /// <summary>
    /// Gets or sets the maximum tokens to generate in response.
    /// </summary>
    public int? MaxTokens { get; init; } = 2048;

    /// <summary>
    /// Gets or sets the stop sequences.
    /// </summary>
    public IReadOnlyList<string>? StopSequences { get; init; } = [];

    // --- 3. Stream Control ---

    /// <summary>
    /// Gets or sets a value indicating whether streaming is enabled.
    /// </summary>
    public bool? StreamingEnabled { get; init; } = true;

    // --- 4. Tool & Function Calling ---

    /// <summary>
    /// Gets or sets the atomic tools available for the model during chat operations.
    /// </summary>
    public IReadOnlyList<IVKAtomicTool>? Tools { get; init; } = [];

    /// <summary>
    /// Gets or sets the tool choice policy ("Auto", "None", "Required", or specific tool name).
    /// Defaults to "Auto".
    /// </summary>
    public string? ToolChoice { get; init; } = "Auto";

    /// <summary>
    /// Gets or sets whether the chat engine should automatically execute tool calls and continue generation.
    /// Defaults to true.
    /// </summary>
    public bool? AutoInvokeTools { get; init; } = true;

    /// <summary>
    /// Gets or sets the maximum rounds of automated tool invocation allowed in a single chat turn.
    /// Hard security ceiling to prevent runaway loops.
    /// </summary>
    [VKNoRequestOverride]
    public int MaxAutoToolRounds { get; init; } = 5;
}
