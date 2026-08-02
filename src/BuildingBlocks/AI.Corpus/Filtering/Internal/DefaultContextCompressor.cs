using System;
using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Corpus.Filtering.Internal;

/// <summary>
/// Default implementation of <see cref="IVKContextCompressor"/> using sentence-boundary detection.
/// Follows CS.01, AP.01.
/// </summary>
internal sealed class DefaultContextCompressor : IVKContextCompressor
{
    private static readonly char[] SentenceDelimiters = ['.', '!', '?', '。', '！', '？', '\n'];

    /// <inheritdoc />
    public VKResult<string> Compress(string parentContent, int startOffset, int endOffset, int sentenceWindow = 2)
    {
        if (string.IsNullOrWhiteSpace(parentContent))
        {
            return VKResult.Success(string.Empty);
        }

        if (startOffset < 0 || endOffset > parentContent.Length || startOffset > endOffset)
        {
            // If offsets are invalid or out of bounds, fallback to full content safely
            return VKResult.Success(parentContent);
        }

        var sentences = SplitIntoSentences(parentContent);
        if (sentences.Count == 0)
        {
            return VKResult.Success(parentContent);
        }

        int targetIndex = -1;
        for (int i = 0; i < sentences.Count; i++)
        {
            var (sStart, sEnd, _) = sentences[i];
            if (startOffset >= sStart && startOffset <= sEnd)
            {
                targetIndex = i;
                break;
            }
        }

        if (targetIndex == -1)
        {
            return VKResult.Success(parentContent);
        }

        int minIdx = Math.Max(0, targetIndex - sentenceWindow);
        int maxIdx = Math.Min(sentences.Count - 1, targetIndex + sentenceWindow);

        int startPos = sentences[minIdx].Start;
        int endPos = sentences[maxIdx].End;

        string extracted = parentContent[startPos..endPos].Trim();
        return VKResult.Success(extracted); // [CS.01]
    }

    private static List<(int Start, int End, string Text)> SplitIntoSentences(string content)
    {
        var result = new List<(int Start, int End, string Text)>();
        int currentStart = 0;

        for (int i = 0; i < content.Length; i++)
        {
            char c = content[i];
            if (Array.IndexOf(SentenceDelimiters, c) >= 0 || i == content.Length - 1)
            {
                int end = i + 1;
                string text = content[currentStart..end];
                result.Add((currentStart, end, text));
                currentStart = end;
            }
        }

        return result;
    }
}
