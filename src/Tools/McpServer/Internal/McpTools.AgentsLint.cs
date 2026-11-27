using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ModelContextProtocol.Server;
using VK.Tools.McpServer.Internal.Agents;
using VK.Tools.McpServer.Internal.Manifest;

namespace VK.Tools.McpServer.Internal;

internal sealed partial class McpTools
{
    [McpServerTool]
    [Description("Lints .agents governance docs and external pitfall files for broken MCP tool names, broken links, missing skill frontmatter/index entries, legacy handshakes, undefined L3 rule IDs, and pitfalls.md heading formatting.")]
    public static async Task<string> VKBeAgentsLint(CancellationToken ct = default)
    {
        try
        {
            var report = await RunAgentsLintAsync(ct).ConfigureAwait(false);
            return FormatAgentsLintReport(report);
        }
        catch (Exception ex)
        {
            return $"[Error] Failed to lint .agents governance docs: {ex.Message}";
        }
    }

    internal static Task<AgentsLintReport> RunAgentsLintAsync(CancellationToken ct) =>
        AgentsGovernanceLinter.RunAsync(FindProjectRoot(), DiscoverRegisteredToolNames(), ct);

    internal static string FormatAgentsLintReport(AgentsLintReport report)
    {
        var sb = new StringBuilder();
        sb.AppendLine(report.HasErrors
            ? $"🚩 [Agents Lint] {report.ErrorCount} error(s), {report.WarningCount} warning(s) across {report.FilesScanned} file(s) ({report.SkillsCount} skills, {report.RegisteredToolsCount} MCP tools)."
            : $"✅ [Agents Lint] 0 errors, {report.WarningCount} warning(s) across {report.FilesScanned} file(s) ({report.SkillsCount} skills, {report.RegisteredToolsCount} MCP tools).");

        foreach (var group in report.Findings.GroupBy(f => f.File).OrderBy(g => g.Key, StringComparer.Ordinal))
        {
            sb.AppendLine();
            sb.AppendLine($"### {group.Key}");
            foreach (var f in group.OrderBy(f => f.Line))
            {
                var level = f.Severity == ManifestSeverity.Error ? "error" : "warn";
                sb.AppendLine($"- {f.File}:{f.Line} [{f.Code}] {level}: {f.Message}");
            }
        }

        return sb.ToString();
    }

    private static HashSet<string> DiscoverRegisteredToolNames()
    {
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var methods = typeof(McpTools).GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);

        foreach (var method in methods)
        {
            var attr = method.GetCustomAttribute<McpServerToolAttribute>();
            if (attr is null)
            {
                continue;
            }

            var toolName = !string.IsNullOrWhiteSpace(attr.Name)
                ? attr.Name
                : ToSnakeCase(method.Name);

            names.Add(toolName);
        }

        return names;
    }

    private static string ToSnakeCase(string pascalName)
    {
        if (string.IsNullOrEmpty(pascalName))
        {
            return pascalName;
        }

        var sb = new StringBuilder(pascalName.Length + 8);
        for (int i = 0; i < pascalName.Length; i++)
        {
            var c = pascalName[i];
            if (char.IsUpper(c))
            {
                var hasPrevLower = i > 0 && char.IsLower(pascalName[i - 1]);
                var isAcronymEnd = i > 0 && char.IsUpper(pascalName[i - 1]) && i + 1 < pascalName.Length && char.IsLower(pascalName[i + 1]);
                if (hasPrevLower || isAcronymEnd)
                {
                    sb.Append('_');
                }

                sb.Append(char.ToLowerInvariant(c));
            }
            else
            {
                sb.Append(c);
            }
        }

        return sb.ToString();
    }
}
