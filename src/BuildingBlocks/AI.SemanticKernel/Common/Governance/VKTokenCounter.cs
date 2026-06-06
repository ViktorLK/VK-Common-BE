using System;
using System.Collections.Concurrent;
using Microsoft.ML.Tokenizers;
using VK.Blocks.Core;

namespace VK.Blocks.AI.SemanticKernel.Common.Governance;

/// <summary>
/// Utility for counting tokens in strings and chat messages.
/// </summary>
public sealed class VKTokenCounter
{
    private static readonly ConcurrentDictionary<string, TiktokenTokenizer> TokenizerCache = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Counts tokens in the specified text.
    /// </summary>
    public static int CountTokens(string text, string? modelId = null)
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
            // Fallback: character length / 4
            return text.Length / 4;
        }
    }

    private static TiktokenTokenizer GetTokenizer(string? modelId)
    {
        var model = modelId ?? "gpt-4o";
        return TokenizerCache.GetOrAdd(model, m => TiktokenTokenizer.CreateForModel(m));
    }
}
