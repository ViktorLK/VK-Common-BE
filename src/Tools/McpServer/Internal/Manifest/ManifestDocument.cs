using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace VK.Tools.McpServer.Internal.Manifest;

/// <summary>
/// Parsed view of a <c>module-manifest.md</c>: the opening tag attributes and the level-2 sections.
/// The body is never parsed as XML because Markdown prose may contain raw '&lt;' and '&amp;'.
/// </summary>
internal sealed partial record ManifestDocument
{
    public required int TagLine { get; init; }

    public required bool IsClosed { get; init; }

    public required IReadOnlyDictionary<string, string> Attributes { get; init; }

    public required IReadOnlyList<ManifestSection> Sections { get; init; }

    /// <summary>
    /// Parses manifest text. Returns <c>null</c> when no opening <c>&lt;manifest&gt;</c> tag exists;
    /// the caller reports that as a finding (tool-internal parser, not a VK.Blocks application layer).
    /// </summary>
    public static ManifestDocument? Parse(string[] lines)
    {
        var text = string.Join('\n', lines);
        var tagMatch = OpeningTagRegex().Match(text);
        if (!tagMatch.Success)
        {
            return null;
        }

        var attributes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (Match attr in AttributeRegex().Matches(tagMatch.Groups[1].Value))
        {
            attributes[attr.Groups[1].Value] = attr.Groups[2].Value.Trim();
        }

        var tagLine = CountLines(text, tagMatch.Index);
        var tagEndLine = CountLines(text, tagMatch.Index + tagMatch.Length);

        var sections = new List<ManifestSection>();
        string? currentTitle = null;
        var currentLine = 0;
        var currentLines = new List<(int, string)>();
        var inComment = false;

        for (var i = tagEndLine; i < lines.Length; i++)
        {
            var raw = lines[i].Trim();
            if (raw.StartsWith("</manifest>", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            var heading = SectionHeadingRegex().Match(raw);
            if (heading.Success)
            {
                Flush();
                currentTitle = heading.Groups[1].Value.Trim();
                currentLine = i + 1;
                continue;
            }

            var content = StripComments(raw, ref inComment);
            if (currentTitle is not null && content.Length > 0)
            {
                currentLines.Add((i + 1, content));
            }
        }

        Flush();

        return new ManifestDocument
        {
            TagLine = tagLine,
            IsClosed = text.Contains("</manifest>", StringComparison.OrdinalIgnoreCase),
            Attributes = attributes,
            Sections = sections,
        };

        void Flush()
        {
            if (currentTitle is null)
            {
                return;
            }

            sections.Add(new ManifestSection { Title = currentTitle, Line = currentLine, Lines = [.. currentLines] });
            currentLines.Clear();
        }
    }

    private static int CountLines(string text, int index)
    {
        var count = 1;
        for (var i = 0; i < index && i < text.Length; i++)
        {
            if (text[i] == '\n')
            {
                count++;
            }
        }

        return count;
    }

    private static string StripComments(string line, ref bool inComment)
    {
        var result = new System.Text.StringBuilder();
        var pos = 0;
        while (pos < line.Length)
        {
            if (inComment)
            {
                var end = line.IndexOf("-->", pos, StringComparison.Ordinal);
                if (end < 0)
                {
                    return result.ToString().Trim();
                }

                inComment = false;
                pos = end + 3;
                continue;
            }

            var start = line.IndexOf("<!--", pos, StringComparison.Ordinal);
            if (start < 0)
            {
                result.Append(line, pos, line.Length - pos);
                break;
            }

            result.Append(line, pos, start - pos);
            inComment = true;
            pos = start + 4;
        }

        return result.ToString().Trim();
    }

    [GeneratedRegex(@"<manifest\b([^>]*)>", RegexOptions.IgnoreCase)]
    private static partial Regex OpeningTagRegex();

    [GeneratedRegex(@"([\w-]+)\s*=\s*""([^""]*)""")]
    private static partial Regex AttributeRegex();

    [GeneratedRegex(@"^##\s+(?:\d+\.\s*)?(.+?)\s*$")]
    private static partial Regex SectionHeadingRegex();
}
