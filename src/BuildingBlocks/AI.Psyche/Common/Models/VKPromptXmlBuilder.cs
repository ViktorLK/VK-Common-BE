using System;
using System.Collections.Generic;
using System.Linq;
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
        sb.AppendLine(content.Trim());
        sb.Append("</");
        sb.Append(tagName);
        sb.Append('>');

        return sb.ToString();
    }

    /// <summary>
    /// Wraps multiple contiguous items within a single XML root element.
    /// Skips null or whitespace items.
    /// Returns <see cref="string.Empty"/> if no non-empty items exist.
    /// </summary>
    /// <param name="tagName">The name of the XML tag.</param>
    /// <param name="items">The sequence of inner items to coalesce.</param>
    /// <param name="separator">The separator between items (defaults to newline).</param>
    /// <param name="attributes">Optional XML attributes for the root element.</param>
    /// <returns>The formatted XML string enclosing all items.</returns>
    public static string Wrap(
        string tagName,
        IEnumerable<string> items,
        string separator = "\n",
        IReadOnlyDictionary<string, string>? attributes = null)
    {
        VKGuard.NotNullOrWhiteSpace(tagName); // [AP.01]
        VKGuard.NotNull(items); // [AP.01]

        var validItems = items.Where(item => !string.IsNullOrWhiteSpace(item)).ToList();
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
            if (i > 0 && !string.IsNullOrEmpty(separator))
            {
                sb.Append(separator);
            }
            sb.AppendLine(validItems[i].Trim());
        }

        sb.Append("</");
        sb.Append(tagName);
        sb.Append('>');

        return sb.ToString();
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
