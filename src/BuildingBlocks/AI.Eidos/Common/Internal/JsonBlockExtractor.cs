using System;
using System.Text.Json;
using System.Text.RegularExpressions;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos.Common.Internal;

/// <summary>
/// Reusable utility for extracting clean JSON candidate payloads from raw LLM output,
/// stripping reasoning chains (such as &lt;think&gt; tags) and isolating markdown code blocks.
/// Thread-safe and hardened against catastrophic regex backtracking.
/// </summary>
internal static class JsonBlockExtractor
{
    // [OR.01] & ReDoS hardened with NonBacktracking where supported
    private static readonly Regex ThinkTagRegex = new(
        @"<think>[\s\S]*?</think>",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.NonBacktracking);

    private static readonly Regex JsonCodeBlockRegex = new(
        @"```(?:json)?\s*((?:\{[\s\S]*?\})|(?:\[[\s\S]*?\]))\s*```",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.NonBacktracking);

    private static readonly Regex MarkdownFenceStartRegex = new(
        @"```(?:json)?\s*",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.NonBacktracking);

    /// <summary>
    /// Strips thinking/reasoning blocks (e.g., &lt;think&gt;...&lt;/think&gt;) from raw LLM text.
    /// </summary>
    public static string StripThinkingBlocks(string rawText)
    {
        if (string.IsNullOrWhiteSpace(rawText))
        {
            return string.Empty;
        }

        return ThinkTagRegex.Replace(rawText, string.Empty).Trim();
    }

    /// <summary>
    /// Extracts the most viable JSON payload from the raw text, preferring markdown fences,
    /// and falling back to outermost brace heuristics.
    /// </summary>
    public static string ExtractJsonBlock(string rawText)
    {
        if (string.IsNullOrWhiteSpace(rawText))
        {
            return string.Empty;
        }

        // 1. Strip reasoning/thinking tags
        var sanitized = StripThinkingBlocks(rawText);
        if (string.IsNullOrWhiteSpace(sanitized))
        {
            return string.Empty;
        }

        // 2. Scan code blocks in reverse order (models output final conclusion/result at the end)
        var matches = JsonCodeBlockRegex.Matches(sanitized);
        if (matches.Count > 0)
        {
            for (int i = matches.Count - 1; i >= 0; i--)
            {
                var candidate = matches[i].Groups[1].Value.Trim();
                if (IsValidJson(candidate))
                {
                    return candidate;
                }
            }
        }

        // 3. Fallback: heuristic scan from outermost enclosing brackets in sanitized text
        var candidateOuter = LocateOuterJsonCandidate(sanitized, requireClosed: true);
        if (!string.IsNullOrWhiteSpace(candidateOuter) && IsValidJson(candidateOuter))
        {
            return candidateOuter;
        }

        return sanitized.Trim();
    }

    /// <summary>
    /// Locates active JSON candidate for streaming parsers where the end bracket may or may not be closed.
    /// </summary>
    public static string LocateStreamingJsonCandidate(string sanitizedText)
    {
        if (string.IsNullOrWhiteSpace(sanitizedText))
        {
            return string.Empty;
        }

        // Check for markdown code fences
        var fenceMatches = MarkdownFenceStartRegex.Matches(sanitizedText);
        if (fenceMatches.Count > 0)
        {
            var lastFence = fenceMatches[fenceMatches.Count - 1];
            var contentStartIndex = lastFence.Index + lastFence.Length;
            var remaining = sanitizedText.Substring(contentStartIndex);

            var endFenceIndex = remaining.IndexOf("```", StringComparison.Ordinal);
            if (endFenceIndex >= 0)
            {
                return remaining.Substring(0, endFenceIndex).Trim();
            }

            return remaining.Trim();
        }

        // Fallback: search for outermost curly or square brace
        return LocateOuterJsonCandidate(sanitizedText, requireClosed: false);
    }

    /// <summary>
    /// Validates if the string parses as a valid JSON object or array.
    /// </summary>
    public static bool IsValidJson(string candidate)
    {
        if (string.IsNullOrWhiteSpace(candidate))
        {
            return false;
        }

        try
        {
            using var doc = JsonDocument.Parse(candidate);
            return doc.RootElement.ValueKind is JsonValueKind.Object or JsonValueKind.Array;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static string LocateOuterJsonCandidate(string text, bool requireClosed)
    {
        var firstCurly = text.IndexOf('{');
        var firstSquare = text.IndexOf('[');

        if (firstCurly >= 0 && (firstSquare < 0 || firstCurly < firstSquare))
        {
            var lastCurly = text.LastIndexOf('}');
            if (lastCurly > firstCurly)
            {
                return text.Substring(firstCurly, lastCurly - firstCurly + 1);
            }

            return requireClosed ? string.Empty : text.Substring(firstCurly);
        }

        if (firstSquare >= 0)
        {
            var lastSquare = text.LastIndexOf(']');
            if (lastSquare > firstSquare)
            {
                return text.Substring(firstSquare, lastSquare - firstSquare + 1);
            }

            return requireClosed ? string.Empty : text.Substring(firstSquare);
        }

        return string.Empty;
    }
}
