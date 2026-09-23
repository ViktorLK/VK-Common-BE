using System;
using System.Collections.Generic;
using System.Linq;
using VK.Blocks.AI.Psyche.Common.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// High-performance XML prompt formatting utility.
/// Provides standardized wrapping, attribute escaping, and multi-item coalesce formatting.
/// Follows AP.01, AP.03, and CS.04 (Span/stackalloc buffering).
/// </summary>
public static class VKPromptXmlBuilder
{
    /// <summary>
    /// Wraps text content in an XML tag with optional attributes.
    /// If the content is already wrapped by the matching tag, it is returned as-is (idempotent).
    /// If the content contains multiple sibling blocks of the same tag, their contents are coalesced into a single tag.
    /// Returns <see cref="string.Empty"/> if content is null or whitespace.
    /// </summary>
    /// <param name="tagName">The name of the XML tag.</param>
    /// <param name="content">The inner text content.</param>
    /// <param name="attributes">Optional XML attributes.</param>
    /// <returns>The formatted XML string.</returns>
    public static string Wrap(
        string tagName,
        string content,
        IReadOnlyDictionary<string, string>? attributes = null)
    {
        VKGuard.NotNullOrWhiteSpace(tagName); // [AP.01]

        if (string.IsNullOrWhiteSpace(content))
        {
            return string.Empty;
        }

        var trimmed = content.Trim();
        var blocks = new List<string>();
        if (TryExtractTagBlocks(tagName, trimmed, blocks))
        {
            if (blocks.Count == 1 && (attributes is null || attributes.Count == 0))
            {
                return trimmed;
            }

            if (blocks.Count > 1)
            {
                return Wrap(tagName, blocks, PsycheConstants.Separators.SegmentSeparator, attributes);
            }

            // Single block with new attributes -> rewrap inner content
            trimmed = blocks[0];
        }

        Span<char> initialBuffer = stackalloc char[512];
        using var sb = new VKValueStringBuilder(initialBuffer);

        sb.Append('<');
        sb.Append(tagName);
        if (attributes is not null && attributes.Count > 0)
        {
            foreach (var (key, value) in attributes)
            {
                sb.Append(' ');
                sb.Append(key);
                sb.Append("=\"");
                sb.Append(value);
                sb.Append('"');
            }
        }
        sb.Append('>');
        sb.AppendLine();
        sb.AppendLine(trimmed);
        sb.Append("</");
        sb.Append(tagName);
        sb.Append('>');

        return sb.ToString();
    }

    /// <summary>
    /// Wraps multiple contiguous items within a single XML root element.
    /// Automatically unwraps any items that are already enclosed in the same <paramref name="tagName"/>.
    /// Skips null or whitespace items.
    /// Returns <see cref="string.Empty"/> if no non-empty items exist.
    /// </summary>
    /// <param name="tagName">The name of the XML tag.</param>
    /// <param name="items">The sequence of inner items to coalesce.</param>
    /// <param name="separator">The separator between items (defaults to <see cref="PsycheConstants.Separators.SegmentSeparator"/>).</param>
    /// <param name="attributes">Optional XML attributes for the root element.</param>
    /// <returns>The formatted XML string enclosing all items.</returns>
    public static string Wrap(
        string tagName,
        IEnumerable<string> items,
        string separator = PsycheConstants.Separators.SegmentSeparator,
        IReadOnlyDictionary<string, string>? attributes = null)
    {
        VKGuard.NotNullOrWhiteSpace(tagName); // [AP.01]
        VKGuard.NotNull(items); // [AP.01]

        var rawItems = items.Where(item => !string.IsNullOrWhiteSpace(item)).ToList();
        if (rawItems.Count == 0)
        {
            return string.Empty;
        }

        if (rawItems.Count == 1 && (attributes is null || attributes.Count == 0))
        {
            var single = rawItems[0].Trim();
            var singleBlocks = new List<string>();
            if (TryExtractTagBlocks(tagName, single, singleBlocks) && singleBlocks.Count == 1)
            {
                return single;
            }
        }

        var validItems = new List<string>(rawItems.Count);
        foreach (var rawItem in rawItems)
        {
            var extractedBlocks = new List<string>();
            if (TryExtractTagBlocks(tagName, rawItem, extractedBlocks))
            {
                validItems.AddRange(extractedBlocks);
            }
            else
            {
                validItems.Add(rawItem.Trim());
            }
        }

        if (validItems.Count == 0)
        {
            return string.Empty;
        }

        Span<char> initialBuffer = stackalloc char[1024];
        using var sb = new VKValueStringBuilder(initialBuffer);

        sb.Append('<');
        sb.Append(tagName);
        if (attributes is not null && attributes.Count > 0)
        {
            foreach (var (key, value) in attributes)
            {
                sb.Append(' ');
                sb.Append(key);
                sb.Append("=\"");
                sb.Append(value);
                sb.Append('"');
            }
        }
        sb.Append('>');
        sb.AppendLine();

        for (int i = 0; i < validItems.Count; i++)
        {
            if (i > 0)
            {
                if (!string.IsNullOrEmpty(separator))
                {
                    sb.Append(separator);
                }
                else
                {
                    sb.AppendLine();
                }
            }
            sb.Append(validItems[i]);
            if (i < validItems.Count - 1 && !string.IsNullOrEmpty(separator) && !separator.EndsWith('\n'))
            {
                sb.AppendLine();
            }
        }

        sb.AppendLine();
        sb.Append("</");
        sb.Append(tagName);
        sb.Append('>');

        return sb.ToString();
    }

    /// <summary>
    /// Unwraps the inner content if the text is enclosed by the specified XML tag.
    /// If multiple sibling blocks of the same tag exist, their inner contents are extracted and joined with the separator.
    /// Returns the original text if not enclosed.
    /// </summary>
    /// <param name="tagName">The name of the XML tag.</param>
    /// <param name="content">The text content to unwrap.</param>
    /// <param name="separator">The separator used if multiple sibling blocks are unwrapped (defaults to <see cref="PsycheConstants.Separators.SegmentSeparator"/>).</param>
    /// <returns>The unwrapped content, or the original content if not enclosed.</returns>
    public static string Unwrap(string tagName, string content, string separator = PsycheConstants.Separators.SegmentSeparator)
    {
        VKGuard.NotNullOrWhiteSpace(tagName); // [AP.01]
        if (string.IsNullOrWhiteSpace(content))
        {
            return string.Empty;
        }

        var blocks = new List<string>();
        if (TryExtractTagBlocks(tagName, content.Trim(), blocks))
        {
            return string.Join(separator, blocks);
        }

        return content;
    }

    /// <summary>
    /// Extracts all top-level blocks of the specified <paramref name="tagName"/> from <paramref name="text"/>.
    /// Returns true only if the non-whitespace parts of <paramref name="text"/> are completely composed of matching blocks.
    /// </summary>
    private static bool TryExtractTagBlocks(string tagName, string text, List<string> blocks)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        var openTagPrefix = $"<{tagName}";
        var closeTag = $"</{tagName}>";

        int index = 0;
        int length = text.Length;

        while (index < length)
        {
            while (index < length && char.IsWhiteSpace(text[index]))
            {
                index++;
            }

            if (index >= length)
            {
                break;
            }

            if (index + openTagPrefix.Length > length ||
                !text.AsSpan(index, openTagPrefix.Length).Equals(openTagPrefix.AsSpan(), StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            int afterPrefix = index + openTagPrefix.Length;
            if (afterPrefix >= length)
            {
                return false;
            }

            char nextChar = text[afterPrefix];
            if (nextChar != '>' && !char.IsWhiteSpace(nextChar))
            {
                return false;
            }

            int openTagEnd = text.IndexOf('>', afterPrefix);
            if (openTagEnd < 0)
            {
                return false;
            }

            int depth = 1;
            int searchPos = openTagEnd + 1;
            int closingTagStart = -1;

            while (searchPos < length && depth > 0)
            {
                int nextLt = text.IndexOf('<', searchPos);
                if (nextLt < 0)
                {
                    break;
                }

                if (nextLt + closeTag.Length <= length &&
                    text.AsSpan(nextLt, closeTag.Length).Equals(closeTag.AsSpan(), StringComparison.OrdinalIgnoreCase))
                {
                    depth--;
                    if (depth == 0)
                    {
                        closingTagStart = nextLt;
                        searchPos = nextLt + closeTag.Length;
                        break;
                    }
                    searchPos = nextLt + closeTag.Length;
                    continue;
                }

                if (nextLt + openTagPrefix.Length < length &&
                    text.AsSpan(nextLt, openTagPrefix.Length).Equals(openTagPrefix.AsSpan(), StringComparison.OrdinalIgnoreCase))
                {
                    char nestedNext = text[nextLt + openTagPrefix.Length];
                    if (nestedNext == '>' || char.IsWhiteSpace(nestedNext))
                    {
                        depth++;
                    }
                }

                searchPos = nextLt + 1;
            }

            if (depth != 0 || closingTagStart < 0)
            {
                return false;
            }

            var innerContent = text.Substring(openTagEnd + 1, closingTagStart - (openTagEnd + 1)).Trim();
            if (!string.IsNullOrWhiteSpace(innerContent))
            {
                blocks.Add(innerContent);
            }

            index = closingTagStart + closeTag.Length;
        }

        return blocks.Count > 0;
    }

    /// <summary>
    /// Formats a self-closing XML tag with attributes (e.g. &lt;tag attr="val" /&gt;).
    /// </summary>
    /// <param name="tagName">The name of the XML tag.</param>
    /// <param name="attributes">The XML attributes.</param>
    /// <returns>The formatted self-closing XML string.</returns>
    public static string WrapSelfClosing(
        string tagName,
        IReadOnlyDictionary<string, string> attributes)
    {
        VKGuard.NotNullOrWhiteSpace(tagName); // [AP.01]
        VKGuard.NotNull(attributes); // [AP.01]

        Span<char> initialBuffer = stackalloc char[256];
        using var sb = new VKValueStringBuilder(initialBuffer);

        sb.Append('<');
        sb.Append(tagName);

        foreach (var (key, value) in attributes)
        {
            sb.Append(' ');
            sb.Append(key);
            sb.Append("=\"");
            sb.Append(value);
            sb.Append('"');
        }

        sb.Append(" />");

        return sb.ToString();
    }
}
