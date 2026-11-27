using System.Collections.Generic;
using System.Linq;
using VK.Tools.McpServer.Internal.Manifest;

namespace VK.Tools.McpServer.Internal.Agents;

/// <summary>
/// Aggregated result of an .agents governance lint run.
/// </summary>
internal sealed record AgentsLintReport
{
    public required int FilesScanned { get; init; }

    public required int SkillsCount { get; init; }

    public required int RegisteredToolsCount { get; init; }

    public required IReadOnlyList<AgentsLintFinding> Findings { get; init; }

    public int ErrorCount => Findings.Count(f => f.Severity == ManifestSeverity.Error);

    public int WarningCount => Findings.Count(f => f.Severity == ManifestSeverity.Warning);

    public bool HasErrors => ErrorCount > 0;
}
