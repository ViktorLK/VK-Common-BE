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
    public static async Task<string> VKBeGetArchitecturalRule(
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

            foreach (var file in ruleFiles)
            {
                var lines = await File.ReadAllLinesAsync(file, ct).ConfigureAwait(false);

                // Find the header line that starts with ### {ruleId}
                var startLineIndex = -1;
                for (int i = 0; i < lines.Length; i++)
                {
                    var trimmed = lines[i].TrimStart();
                    if (trimmed.StartsWith($"### {ruleId}", StringComparison.OrdinalIgnoreCase))
                    {
                        startLineIndex = i;
                        break;
                    }
                }

                if (startLineIndex == -1)
                    continue;

                // Find the end line (next level 3 header ### or end of file)
                var endLineIndex = lines.Length;
                for (int i = startLineIndex + 1; i < lines.Length; i++)
                {
                    var trimmed = lines[i].TrimStart();
                    if (trimmed.StartsWith("### ", StringComparison.OrdinalIgnoreCase) && !trimmed.StartsWith("####", StringComparison.OrdinalIgnoreCase))
                    {
                        endLineIndex = i;
                        break;
                    }
                }

                var ruleLines = lines[startLineIndex..endLineIndex];
                var ruleContent = string.Join("\n", ruleLines).Trim();

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

