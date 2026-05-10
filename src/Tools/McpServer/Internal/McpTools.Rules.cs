using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ModelContextProtocol.Server;

namespace VK.Tools.McpServer.Internal;

internal sealed partial class McpTools
{
    [McpServerTool]
    [Description("Retrieves the detailed specification of a specific VK.Blocks architectural rule by its logical ID (e.g., 'CS.01', 'OR.01').")]
    public static async Task<string> VKGetArchitecturalRule(
        [Description("The logical ID of the rule to retrieve (e.g., 'CS.01').")] string ruleId,
        CancellationToken ct)
    {
        var result = await GetRuleContentInternal(ruleId, ct).ConfigureAwait(false);
        if (result == null)
        {
            return $"[Error] Rule ID '{ruleId}' not found in any definition file.";
        }
        return result;
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

                // Find the start of the next header (###)
                var nextHeaderIndex = content.IndexOf("### ", startIndex + targetHeader.Length, StringComparison.OrdinalIgnoreCase);

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
