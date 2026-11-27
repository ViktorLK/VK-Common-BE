using VK.Tools.McpServer.Internal.Manifest;

namespace VK.Tools.McpServer.Internal.Agents;

/// <summary>
/// A single governance lint finding located at a repository-relative file and 1-based line number.
/// </summary>
internal sealed record AgentsLintFinding
{
    public required string Code { get; init; }

    public required ManifestSeverity Severity { get; init; }

    public required string File { get; init; }

    public required int Line { get; init; }

    public required string Message { get; init; }
}
