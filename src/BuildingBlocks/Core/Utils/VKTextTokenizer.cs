using System;
using System.Collections.Generic;

namespace VK.Blocks.Core;

/// <summary>
/// Utility class providing standardized text tokenization and line splitting operations.
/// Follows AP.01, AP.03.
/// </summary>
public static class VKTextTokenizer
{
    /// <summary>
    /// Default delimiter characters for word tokenization including punctuation, whitespace, and line breaks.
    /// </summary>
    public static readonly char[] DefaultWordDelimiters =
    [
        ' ', '.', ',', '!', '?', ';', ':', '-', '(', ')', '[', ']', '{', '}', '\r', '\n', '\t'
    ];

    /// <summary>
    /// Line break characters for splitting text by lines.
    /// </summary>
    public static readonly char[] LineDelimiters = ['\r', '\n'];

    /// <summary>
    /// Tokenizes the specified text into non-empty words using default delimiters.
    /// </summary>
    /// <param name="text">The text to tokenize.</param>
    /// <returns>A list of non-empty tokens.</returns>
    public static IReadOnlyList<string> TokenizeWords(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return [];
        }

        return text.Split(DefaultWordDelimiters, StringSplitOptions.RemoveEmptyEntries);
    }

    /// <summary>
    /// Tokenizes the specified text into non-empty words using custom delimiters.
    /// </summary>
    /// <param name="text">The text to tokenize.</param>
    /// <param name="delimiters">Custom delimiter characters.</param>
    /// <returns>A list of non-empty tokens.</returns>
    public static IReadOnlyList<string> TokenizeWords(string? text, char[] delimiters)
    {
        VKGuard.NotNull(delimiters);

        if (string.IsNullOrWhiteSpace(text))
        {
            return [];
        }

        return text.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
    }

    /// <summary>
    /// Splits the specified text into non-empty lines, stripping carriage returns and newlines.
    /// </summary>
    /// <param name="text">The text to split.</param>
    /// <returns>A list of non-empty lines.</returns>
    public static IReadOnlyList<string> SplitLines(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return [];
        }

        return text.Split(LineDelimiters, StringSplitOptions.RemoveEmptyEntries);
    }
}
