namespace VK.Blocks.AI;

/// <summary>
/// Defines generative parameters for AI requests.
/// </summary>
public interface IVKGenerationOptions
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
}
