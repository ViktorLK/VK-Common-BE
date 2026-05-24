using System;
using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Common.Internal;

/// <summary>
/// Internal implementation of <see cref="IVKTokenMeter"/> using a lightweight local character-to-token ratio approximation.
/// Follows AP.01 (sealed class default) and AP.03 (internal scoping, no VK prefix).
/// </summary>
/// <remarks>
/// [ADR-001 Option B Implementation]
/// Note: To upgrade to Option A (Absolute BPE Tokenization using Tiktoken):
/// 1. Install Microsoft.DeepDev.Tiktoken or Microsoft.SemanticKernel.Connectors.OpenAI package.
/// 2. Inject and use the specific BPE tokenizer (e.g., Cl100kBase / O200kBase encoder).
/// 3. Replace the counting logic below with: `_tiktokenEncoder.CountTokens(text)`.
/// </remarks>
internal sealed class LocalTokenMeter : IVKTokenMeter
{
    // Conservative average characters per token to prevent under-estimation.
    private const double EnglishCharsPerToken = 3.0;
    private const double CjkCharsPerToken = 0.75;

    // Standard OpenAI chat template overhead per message (e.g., "<|im_start|>role\ncontent<|im_end|>\n")
    private const int MessageOverheadTokens = 4;

    /// <inheritdoc />
    public int CountTokens(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return 0;
        }

        int cjkCount = 0;
        int otherCount = 0;

        foreach (char c in text)
        {
            // Simple heuristic to check for CJK Unified Ideographs or Kana/Hangul ranges
            if ((c >= 0x3000 && c <= 0x9FFF) || (c >= 0xAC00 && c <= 0xD7AF))
            {
                cjkCount++;
            }
            else
            {
                otherCount++;
            }
        }

        double estimatedTokens = (cjkCount / CjkCharsPerToken) + (otherCount / EnglishCharsPerToken);
        return (int)Math.Ceiling(estimatedTokens);
    }

    /// <inheritdoc />
    public int CountTokens(IEnumerable<VKChatMessage> messages)
    {
        VKGuard.NotNull(messages);

        int totalTokens = 0;
        foreach (var message in messages)
        {
            if (message is null)
            {
                continue;
            }

            totalTokens += MessageOverheadTokens;
            totalTokens += CountTokens(message.Role.ToString());

            if (!string.IsNullOrEmpty(message.Content))
            {
                totalTokens += CountTokens(message.Content);
            }

            if (!string.IsNullOrEmpty(message.ReasoningContent))
            {
                totalTokens += CountTokens(message.ReasoningContent);
            }
        }

        // Add prompt prefix overhead
        totalTokens += 3;

        return totalTokens;
    }
}
