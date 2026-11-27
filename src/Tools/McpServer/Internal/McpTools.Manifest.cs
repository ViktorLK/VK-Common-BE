using System;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ModelContextProtocol.Server;
using VK.Tools.McpServer.Internal.Manifest;

namespace VK.Tools.McpServer.Internal;

internal sealed partial class McpTools
{
    private const string DefaultManifestPattern = "AI*";
    private const string DefaultOwnershipMapPath = "docs/03-Architecture/AI/ownership-map.md";

    [McpServerTool]
    [Description("Lints module-manifest.md files: schema (attributes, sections, limits, invariant IDs), csproj references against 'allowed'/'forbidden', and Owns/Delegates against the ownership map.")]
    public static async Task<string> VKBeManifestLint(
        [Description("Comma-separated BuildingBlock directory names or prefix globs (e.g. 'AI*' or 'AI.Synapse,AI.MEAI'). Default: 'AI*'.")] string? modules = null,
        [Description("Ownership map path, repository-relative. Default: 'docs/03-Architecture/AI/ownership-map.md'.")] string? mapPath = null,
        CancellationToken ct = default)
    {
        try
        {
            var report = await RunManifestLintAsync(modules, mapPath, ct).ConfigureAwait(false);
            return FormatManifestReport(report);
        }
        catch (Exception ex)
        {
            return $"[Error] Failed to lint manifests: {ex.Message}";
        }
    }

    internal static Task<ManifestLintReport> RunManifestLintAsync(string? modules, string? mapPath, CancellationToken ct) =>
        ManifestLinter.RunAsync(
            FindProjectRoot(),
            string.IsNullOrWhiteSpace(modules) ? DefaultManifestPattern : modules,
            string.IsNullOrWhiteSpace(mapPath) ? DefaultOwnershipMapPath : mapPath,
            ct);

    internal static string FormatManifestReport(ManifestLintReport report)
    {
        var sb = new StringBuilder();
        sb.AppendLine(report.HasErrors
            ? $"🚩 [Manifest Lint] {report.ErrorCount} error(s), {report.WarningCount} warning(s) across {report.Modules.Count} module(s)."
            : $"✅ [Manifest Lint] 0 errors, {report.WarningCount} warning(s) across {report.Modules.Count} module(s).");
        sb.AppendLine($"Modules: {string.Join(", ", report.Modules)}");

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
}
