using System.Text.RegularExpressions;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos.Parsing.Internal;

internal sealed class DefaultContractExtractor : IVKContractExtractor
{
    private static readonly Regex JsonCodeBlockRegex = new(
        @"```(?:json)?\s*(\{[\s\S]*?\})\s*```",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string ExtractJsonBlock(string rawText)
    {
        if (string.IsNullOrWhiteSpace(rawText)) return string.Empty;

        var match = JsonCodeBlockRegex.Match(rawText);
        if (match.Success && match.Groups.Count > 1)
        {
            return match.Groups[1].Value.Trim();
        }

        var firstBrace = rawText.IndexOf('{');
        var lastBrace = rawText.LastIndexOf('}');
        if (firstBrace >= 0 && lastBrace > firstBrace)
        {
            return rawText.Substring(firstBrace, lastBrace - firstBrace + 1);
        }

        return rawText.Trim();
    }
}
