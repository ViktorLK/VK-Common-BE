namespace VK.Blocks.AI;

/// <summary>
/// Defines the contract for an AI Tokenizer.
/// Handles counting and encoding/decoding of tokens for specific models.
/// </summary>
public interface IVKTokenCounter
{
    /// <summary>
    /// Counts the number of tokens in the given text for a specific model.
    /// </summary>
    /// <param name="text">The input text.</param>
    /// <param name="modelId">The target model ID.</param>
    /// <returns>The token count.</returns>
    int CountTokens(string text, string? modelId = null);
}
