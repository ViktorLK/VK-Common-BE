using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.ML.Tokenizers;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Tokenics.Counting.Internal;

/// <summary>
/// High-precision tokenizer based on Microsoft.ML.Tokenizers.
/// </summary>
internal sealed class DefaultTokenizer : IVKTokenizer
{
    private readonly ConcurrentDictionary<string, Tokenizer> _tokenizers = new(StringComparer.OrdinalIgnoreCase);

    private Tokenizer GetTokenizer(string? modelId)
    {
        var targetModel = string.IsNullOrWhiteSpace(modelId) ? "gpt-4" : modelId;

        return _tokenizers.GetOrAdd(targetModel, model =>
        {
            try
            {
                return TiktokenTokenizer.CreateForModel(model);
            }
            catch
            {
                // Fallback to gpt-4 if the specific model isn't supported
                return TiktokenTokenizer.CreateForModel("gpt-4");
            }
        });
    }

    /// <inheritdoc />
    public int CountTokens(string text, string? modelId = null)
    {
        if (string.IsNullOrEmpty(text))
        {
            return 0;
        }

        var tokenizer = GetTokenizer(modelId);
        return tokenizer.CountTokens(text);
    }

    /// <inheritdoc />
    public IReadOnlyList<int> Encode(string text, string? modelId = null)
    {
        if (string.IsNullOrEmpty(text))
        {
            return [];
        }

        var tokenizer = GetTokenizer(modelId);
        return tokenizer.EncodeToIds(text);
    }

    /// <inheritdoc />
    public string Decode(IEnumerable<int> tokens, string? modelId = null)
    {
        VKGuard.NotNull(tokens);

        var tokenizer = GetTokenizer(modelId);
        return tokenizer.Decode(tokens);
    }
}
