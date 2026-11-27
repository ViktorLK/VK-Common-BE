namespace VK.Tools.McpServer.Internal.Manifest;

/// <summary>
/// Severity of a manifest lint finding. Only <see cref="Error"/> fails the lint run.
/// </summary>
internal enum ManifestSeverity
{
    Warning = 0,
    Error = 1,
}
