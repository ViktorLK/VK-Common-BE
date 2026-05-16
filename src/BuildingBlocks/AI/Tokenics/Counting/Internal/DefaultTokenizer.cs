using System;
using System.Collections.Generic;

namespace VK.Blocks.AI.Tokenics.Counting.Internal;

/// <summary>
/// A default heuristic-based tokenizer that estimates token count based on string length.
/// Useful for general estimation when a model-specific tokenizer is not available.
/// </summary>
internal sealed class DefaultTokenizer : IVKTokenizer
{
    private const float AverageCharsPerToken = 4.0f;

    /// <inheritdoc />
    public int CountTokens(string text, string? modelId = null)
    {
        if (string.IsNullOrEmpty(text))
            return 0;

        // Simple heuristic: 1 token ~= 4 characters for English/Standard text.
        return (int)Math.Ceiling(text.Length / AverageCharsPerToken);
    }

    /// <inheritdoc />
    public IReadOnlyList<int> Encode(string text, string? modelId = null)
    {
        // Default implementation does not support ID encoding.
        return [];
    }

    /// <inheritdoc />
    public string Decode(IEnumerable<int> tokens, string? modelId = null)
    {
        // Default implementation does not support ID decoding.
        return string.Empty;
    }
}
