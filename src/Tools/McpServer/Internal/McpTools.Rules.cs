using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ModelContextProtocol.Server;

namespace VK.Tools.McpServer.Internal;

internal sealed partial class McpTools
{
    [McpServerTool]
    [Description("Retrieves the detailed specifications of VK.Blocks architectural rules by their logical IDs (e.g., 'CS.01', 'OR.01'). Supports comma-separated IDs for batch retrieval.")]
    public static async Task<string> VKGetArchitecturalRule(
        [Description("The logical ID(s) of the rules to retrieve (e.g., 'CS.01' or 'BB.01,BB.02').")] string ruleIds,
        CancellationToken ct)
    {
        var ids = ruleIds.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToArray();
        if (ids.Length == 0)
        {
            return "[Error] No rule IDs provided.";
        }

        var results = new System.Collections.Generic.List<string>();
        foreach (var id in ids)
        {
            var result = await GetRuleContentInternal(id, ct).ConfigureAwait(false);
            results.Add(result ?? $"[Error] Rule ID '{id}' not found in any definition file.");
        }

        return string.Join("\n\n---\n\n", results);
    }

    private static async Task<string?> GetRuleContentInternal(string ruleId, CancellationToken ct)
    {
        try
        {
            var projectRoot = FindProjectRoot();
            var rulesDir = Path.Combine(projectRoot, ".agents", "rules");

            if (!Directory.Exists(rulesDir))
            {
                return null;
            }

            var ruleFiles = Directory.GetFiles(rulesDir, "0*.md");
            var targetHeader = $"### {ruleId}";

            foreach (var file in ruleFiles)
            {
                var content = await File.ReadAllTextAsync(file, ct).ConfigureAwait(false);

                // Find the header (e.g., ### CS.01)
                var startIndex = content.IndexOf(targetHeader, StringComparison.OrdinalIgnoreCase);
                if (startIndex == -1)
                    continue;

                // Find the start of the next header (exactly ### followed by a space)
                // We skip the current header by starting the search after the current line
                var lineEndIndex = content.IndexOf('\n', startIndex);
                if (lineEndIndex == -1) lineEndIndex = startIndex + targetHeader.Length;

                var nextHeaderIndex = -1;
                var searchIndex = lineEndIndex;
                while ((searchIndex = content.IndexOf("### ", searchIndex, StringComparison.OrdinalIgnoreCase)) != -1)
                {
                    // Ensure it's exactly ### (Level 3), not #### (Level 4+)
                    if (searchIndex == 0 || content[searchIndex - 1] == '\n')
                    {
                        // Check if it's ####
                        if (content.Length > searchIndex + 4 && content[searchIndex + 3] != '#')
                        {
                            nextHeaderIndex = searchIndex;
                            break;
                        }
                    }
                    searchIndex += 4;
                }

                string ruleContent;
                if (nextHeaderIndex != -1)
                {
                    ruleContent = content.Substring(startIndex, nextHeaderIndex - startIndex).Trim();
                }
                else
                {
                    ruleContent = content.Substring(startIndex).Trim();
                }

                return $"[Architectural Rule: {ruleId}]\nSource: {Path.GetFileName(file)}\n\n{ruleContent}";
            }

            return null;
        }
        catch (Exception ex)
        {
            return $"[Error] Failed to retrieve architectural rule '{ruleId}': {ex.Message}";
        }
    }
}
