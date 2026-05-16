namespace VK.Blocks.AI;

/// <summary>
/// Defines chat-specific parameters that can be overridden at the request level.
/// </summary>
public interface IVKChatOverrides
{
    /// <summary>
    /// Gets the temperature for generation (0.0 to 1.0).
    /// </summary>
    float? Temperature { get; init; }

    /// <summary>
    /// Gets the TopP value for generation.
    /// </summary>
    float? TopP { get; init; }

    /// <summary>
    /// Gets the frequency penalty.
    /// </summary>
    float? FrequencyPenalty { get; init; }

    /// <summary>
    /// Gets the presence penalty.
    /// </summary>
    float? PresencePenalty { get; init; }

    /// <summary>
    /// Gets the maximum number of tokens to generate.
    /// </summary>
    int? MaxTokens { get; init; }

    /// <summary>
    /// Gets the stop sequences.
    /// </summary>
    System.Collections.Generic.IReadOnlyList<string>? StopSequences { get; init; }

    /// <summary>
    /// Gets a value indicating whether streaming is enabled.
    /// </summary>
    bool? StreamingEnabled { get; init; }

    /// <summary>
    /// Gets the default system prompt to be injected if no system message is present in history.
    /// </summary>
    string? DefaultSystemPrompt { get; init; }

    /// <summary>
    /// Gets the maximum number of history messages to retain.
    /// </summary>
    int? MaxHistoryMessages { get; init; }

    /// <summary>
    /// Gets the tools available for the chat engine.
    /// </summary>
    System.Collections.Generic.IReadOnlyList<IVKAtomicTool>? Tools { get; init; }
}
