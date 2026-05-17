namespace VK.Blocks.AI;

/// <summary>
/// Defines text-specific parameters that can be overridden at the request level.
/// </summary>
public interface IVKTextOverrides :
    IVKAIProviderOverrides,
    IVKAIGovernanceOverrides
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
    /// Gets the maximum number of tokens to generate.
    /// </summary>
    int? MaxTokens { get; init; }

    /// <summary>
    /// Gets the frequency penalty.
    /// </summary>
    float? FrequencyPenalty { get; init; }

    /// <summary>
    /// Gets the presence penalty.
    /// </summary>
    float? PresencePenalty { get; init; }

    /// <summary>
    /// Gets the stop sequences.
    /// </summary>
    System.Collections.Generic.IReadOnlyList<string>? StopSequences { get; init; }
}
