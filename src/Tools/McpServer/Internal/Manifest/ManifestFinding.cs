namespace VK.Tools.McpServer.Internal.Manifest;

/// <summary>
/// A single lint finding located at a file and 1-based line number (0 when not line-specific).
/// </summary>
internal sealed record ManifestFinding
{
    public required string Code { get; init; }

    public required ManifestSeverity Severity { get; init; }

    public required string File { get; init; }

    public required int Line { get; init; }

    public required string Message { get; init; }
}
