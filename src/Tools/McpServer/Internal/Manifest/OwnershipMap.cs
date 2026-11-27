using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace VK.Tools.McpServer.Internal.Manifest;

/// <summary>
/// Parsed ownership map: capability rows under <c>## Ownership Rows</c> and the
/// invariant prefix table under <c>## Invariant Prefix Registry</c>.
/// </summary>
internal sealed partial record OwnershipMap
{
    public const string HostOwner = "Host";

    public const string UnresolvedOwner = "TBD";

    public required IReadOnlyList<OwnershipRow> Rows { get; init; }

    /// <summary>Module name → (prefix, line).</summary>
    public required IReadOnlyDictionary<string, (string Prefix, int Line)> Prefixes { get; init; }

    public static OwnershipMap Parse(string[] lines)
    {
        var rows = new List<OwnershipRow>();
        var prefixes = new Dictionary<string, (string, int)>(StringComparer.OrdinalIgnoreCase);
        var section = string.Empty;

        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (line.StartsWith("## ", StringComparison.Ordinal))
            {
                section = line[3..].Trim();
                continue;
            }

            if (!line.StartsWith('|'))
            {
                continue;
            }

            var cells = line.Trim('|').Split('|');
            for (var c = 0; c < cells.Length; c++)
            {
                cells[c] = cells[c].Trim().Trim('`');
            }

            if (section.Equals("Ownership Rows", StringComparison.OrdinalIgnoreCase)
                && cells.Length >= 3
                && RowIdRegex().IsMatch(cells[0]))
            {
                rows.Add(new OwnershipRow { Id = cells[0], Capability = cells[1], Owner = cells[2], Line = i + 1 });
            }
            else if (section.Equals("Invariant Prefix Registry", StringComparison.OrdinalIgnoreCase)
                     && cells.Length >= 2
                     && PrefixRegex().IsMatch(cells[0]))
            {
                prefixes[cells[1]] = (cells[0], i + 1);
            }
        }

        return new OwnershipMap { Rows = rows, Prefixes = prefixes };
    }

    [GeneratedRegex(@"^OWN-[A-Z0-9-]+$")]
    public static partial Regex RowIdRegex();

    [GeneratedRegex(@"^[A-Z]{2,6}$")]
    private static partial Regex PrefixRegex();
}
