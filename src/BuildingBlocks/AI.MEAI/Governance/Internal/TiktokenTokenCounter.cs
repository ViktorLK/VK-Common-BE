using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.ML.Tokenizers;
using VK.Blocks.Core;

namespace VK.Blocks.AI.MEAI.Governance.Internal;

/// <summary>
/// High-precision token counter and encoder utilizing Microsoft.ML.Tokenizers and an in-memory model cache.
/// Adheres to CS.01 and AP.01.
/// </summary>
internal sealed class TiktokenTokenCounter : IVKTokenCounter, IVKTokenizer
{
    private readonly ConcurrentDictionary<string, TiktokenTokenizer> _cache = new(StringComparer.OrdinalIgnoreCase);

    public int CountTokens(string text, string? modelId = null)
    {
        if (string.IsNullOrEmpty(text))
        {
            return 0;
        }

        try
        {
            var tokenizer = GetTokenizer(modelId);
            return tokenizer.CountTokens(text);
        }
        catch
        {
            // Heuristic fallback in case of unexpected tokenization failure
            return Math.Max(1, text.Length / 4);
        }
    }

    public IReadOnlyList<int> Encode(string text, string? modelId = null)
    {
        if (string.IsNullOrEmpty(text))
        {
            return [];
        }

        return GetTokenizer(modelId).EncodeToIds(text);
    }

    public string Decode(IEnumerable<int> tokens, string? modelId = null)
    {
        VKGuard.NotNull(tokens);
        return GetTokenizer(modelId).Decode(tokens) ?? string.Empty;
    }

    private TiktokenTokenizer GetTokenizer(string? modelId)
    {
        var targetModel = string.IsNullOrWhiteSpace(modelId) ? "gpt-4o" : modelId;

        return _cache.GetOrAdd(targetModel, static key =>
        {
            try
            {
                return TiktokenTokenizer.CreateForModel(key);
            }
            catch (ArgumentException)
            {
                // Fall back to o200k_base or cl100k_base for non-OpenAI or unrecognized model IDs
                try
                {
                    return TiktokenTokenizer.CreateForEncoding("o200k_base");
                }
                catch
                {
                    return TiktokenTokenizer.CreateForEncoding("cl100k_base");
                }
            }
        });
    }
}
