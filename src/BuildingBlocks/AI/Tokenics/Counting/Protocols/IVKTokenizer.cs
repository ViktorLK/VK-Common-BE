using System.Collections.Generic;

namespace VK.Blocks.AI;

/// <summary>
/// Defines the contract for an AI Tokenizer.
/// Handles counting and encoding/decoding of tokens for specific models.
/// </summary>
public interface IVKTokenizer
{
    /// <summary>
    /// Encodes text into a sequence of token IDs.
    /// </summary>
    /// <param name="text">The input text.</param>
    /// <param name="modelId">The target model ID.</param>
    /// <returns>The sequence of token IDs.</returns>
    IReadOnlyList<int> Encode(string text, string? modelId = null);

    /// <summary>
    /// Decodes a sequence of token IDs back into text.
    /// </summary>
    /// <param name="tokens">The sequence of token IDs.</param>
    /// <param name="modelId">The target model ID.</param>
    /// <returns>The decoded text.</returns>
    string Decode(IEnumerable<int> tokens, string? modelId = null);
}
