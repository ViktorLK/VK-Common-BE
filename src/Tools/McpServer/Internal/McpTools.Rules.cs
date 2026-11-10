using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using ModelContextProtocol.Server;

namespace VK.Tools.McpServer.Internal;

internal sealed partial class McpTools
{
    [McpServerTool]
    [Description("Retrieves the detailed specifications of VK.Blocks L3 architectural rules by their logical IDs (e.g., 'CS.01', 'OR.01'). Excludes globally resident L0-L2 rules (BASE, ARCH, FS). Supports comma-separated IDs for batch retrieval.")]
    public static async Task<string> VKBeGetArchitecturalRule(
        [Description("The logical ID(s) of the L3 rules to retrieve (e.g., 'CS.01' or 'BB.01,BB.02'). Do not query L0-L2 rules as they are globally resident.")] string ruleIds,
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
            if (id.StartsWith("BASE-", StringComparison.OrdinalIgnoreCase) ||
                id.StartsWith("ARCH-", StringComparison.OrdinalIgnoreCase) ||
                id.StartsWith("FS-", StringComparison.OrdinalIgnoreCase))
            {
                results.Add($"[Info] Rule ID '{id}' is a globally resident L0-L2 rule. It is already loaded in full in the prompt context and does not require dynamic retrieval.");
                continue;
            }

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

            var ruleFiles = Directory.GetFiles(rulesDir, "*.md")
                .Where(f =>
                {
                    var name = Path.GetFileName(f);
                    return !name.StartsWith("l0-", StringComparison.OrdinalIgnoreCase) &&
                           !name.StartsWith("l1-", StringComparison.OrdinalIgnoreCase) &&
                           !name.StartsWith("l2-", StringComparison.OrdinalIgnoreCase) &&
                           !string.Equals(name, "l3-vk-be.md", StringComparison.OrdinalIgnoreCase) &&
                           !string.Equals(name, "vk-blocks-checklist.md", StringComparison.OrdinalIgnoreCase);
                })
                .ToArray();

            foreach (var file in ruleFiles)
            {
                var content = await File.ReadAllTextAsync(file, ct).ConfigureAwait(false);

                // 1. Match XML rule tag: <rule id="{ruleId}" ...>...</rule>
                var xmlPattern = $@"<rule\s+[^>]*?id\s*=\s*""{Regex.Escape(ruleId)}""[^>]*>([\s\S]*?)</rule>";
                var xmlMatch = Regex.Match(content, xmlPattern, RegexOptions.IgnoreCase);
                if (xmlMatch.Success)
                {
                    var ruleInner = xmlMatch.Groups[1].Value.Trim();
                    return $"[Architectural Rule: {ruleId}]\nSource: {Path.GetFileName(file)}\n\n{ruleInner}";
                }

                // 2. Fallback to Markdown header ### {ruleId}
                var lines = content.Split('\n');
                var startLineIndex = -1;
                for (int i = 0; i < lines.Length; i++)
                {
                    var trimmed = lines[i].Trim();
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
                    var trimmed = lines[i].Trim();
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
