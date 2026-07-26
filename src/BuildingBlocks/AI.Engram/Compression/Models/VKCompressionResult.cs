using System;
using System.Collections.Generic;
using System.Text;

namespace VK.Blocks.AI.Engram;

/// <summary>
/// Structured memory compression result fragment.
/// </summary>
public sealed record VKCompressionResult
{
    public string Narrative { get; init; } = string.Empty;
    public string Facts { get; init; } = string.Empty;
    public string Graph { get; init; } = string.Empty;
    public string? Timeline { get; init; }
    public string? Contradictions { get; init; }
    public string? ActionItems { get; init; }
    public string? Confidence { get; init; }
    public string? PredictiveCue { get; init; }
    public string? EmotionalTagging { get; init; }

    /// <summary>
    /// Formats the structured result into a standardized block string.
    /// </summary>
    public string ToFormattedSummary()
    {
        var sb = new StringBuilder();
        
        if (!string.IsNullOrWhiteSpace(Narrative))
        {
            sb.AppendLine("===NARRATIVE===");
            sb.AppendLine(Narrative.Trim());
        }

        if (!string.IsNullOrWhiteSpace(Facts))
        {
            sb.AppendLine("===FACTS===");
            sb.AppendLine(Facts.Trim());
        }

        if (!string.IsNullOrWhiteSpace(Graph))
        {
            sb.AppendLine("===GRAPH===");
            sb.AppendLine(Graph.Trim());
        }

        if (!string.IsNullOrWhiteSpace(Timeline))
        {
            sb.AppendLine("===TIMELINE===");
            sb.AppendLine(Timeline.Trim());
        }

        if (!string.IsNullOrWhiteSpace(Contradictions))
        {
            sb.AppendLine("===CONTRADICTIONS===");
            sb.AppendLine(Contradictions.Trim());
        }

        if (!string.IsNullOrWhiteSpace(ActionItems))
        {
            sb.AppendLine("===ACTION_ITEMS===");
            sb.AppendLine(ActionItems.Trim());
        }

        if (!string.IsNullOrWhiteSpace(Confidence))
        {
            sb.AppendLine("===CONFIDENCE===");
            sb.AppendLine(Confidence.Trim());
        }

        if (!string.IsNullOrWhiteSpace(PredictiveCue))
        {
            sb.AppendLine("===CUES===");
            sb.AppendLine(PredictiveCue.Trim());
        }

        if (!string.IsNullOrWhiteSpace(EmotionalTagging))
        {
            sb.AppendLine("===EMOTION===");
            sb.AppendLine(EmotionalTagging.Trim());
        }

        return sb.ToString().Trim();
    }

    /// <summary>
    /// Parses raw LLM text output into a structured <see cref="VKCompressionResult"/>.
    /// </summary>
    public static VKCompressionResult Parse(string rawOutput)
    {
        if (string.IsNullOrWhiteSpace(rawOutput))
        {
            return new VKCompressionResult();
        }

        string cleaned = CleanMarkdownWrappers(rawOutput);

        return new VKCompressionResult
        {
            Narrative = ExtractBlock(cleaned, "===NARRATIVE===") ?? (cleaned.Contains("===") ? string.Empty : cleaned),
            Facts = ExtractBlock(cleaned, "===FACTS===") ?? string.Empty,
            Graph = ExtractBlock(cleaned, "===GRAPH===") ?? string.Empty,
            Timeline = ExtractBlock(cleaned, "===TIMELINE==="),
            Contradictions = ExtractBlock(cleaned, "===CONTRADICTIONS==="),
            ActionItems = ExtractBlock(cleaned, "===ACTION_ITEMS==="),
            Confidence = ExtractBlock(cleaned, "===CONFIDENCE==="),
            PredictiveCue = ExtractBlock(cleaned, "===CUES==="),
            EmotionalTagging = ExtractBlock(cleaned, "===EMOTION===")
        };
    }

    private static string CleanMarkdownWrappers(string text)
    {
        string trimmed = text.Trim();
        if (trimmed.StartsWith("```"))
        {
            int firstLineEnd = trimmed.IndexOf('\n');
            if (firstLineEnd != -1)
            {
                trimmed = trimmed[firstLineEnd..].Trim();
            }
            if (trimmed.EndsWith("```"))
            {
                trimmed = trimmed[..^3].Trim();
            }
        }
        return trimmed;
    }

    private static string? ExtractBlock(string content, string blockHeader)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return null;
        }

        int startIndex = content.IndexOf(blockHeader, StringComparison.OrdinalIgnoreCase);
        if (startIndex == -1)
        {
            return null;
        }

        startIndex += blockHeader.Length;
        while (startIndex < content.Length && (content[startIndex] == '\r' || content[startIndex] == '\n'))
        {
            startIndex++;
        }

        int endIndex = content.Length;
        string[] allHeaders = ["===NARRATIVE===", "===FACTS===", "===GRAPH===", "===TIMELINE===", "===CONTRADICTIONS===", "===ACTION_ITEMS===", "===CONFIDENCE===", "===CUES===", "===EMOTION==="];

        foreach (var header in allHeaders)
        {
            if (header.Equals(blockHeader, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            int nextIndex = content.IndexOf(header, startIndex, StringComparison.OrdinalIgnoreCase);
            if (nextIndex != -1 && nextIndex < endIndex)
            {
                endIndex = nextIndex;
            }
        }

        string blockContent = content[startIndex..endIndex].Trim();
        return string.IsNullOrWhiteSpace(blockContent) ? null : blockContent;
    }
}
