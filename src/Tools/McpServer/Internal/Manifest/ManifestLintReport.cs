using System.Collections.Generic;
using System.Linq;

namespace VK.Tools.McpServer.Internal.Manifest;

/// <summary>
/// Aggregated result of a manifest lint run.
/// </summary>
internal sealed record ManifestLintReport
{
    public required IReadOnlyList<string> Modules { get; init; }

    public required IReadOnlyList<ManifestFinding> Findings { get; init; }

    public int ErrorCount => Findings.Count(f => f.Severity == ManifestSeverity.Error);

    public int WarningCount => Findings.Count(f => f.Severity == ManifestSeverity.Warning);

    public bool HasErrors => ErrorCount > 0;
}
