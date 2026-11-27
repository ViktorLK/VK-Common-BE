using System.Collections.Generic;

namespace VK.Tools.McpServer.Internal.Manifest;

/// <summary>
/// A level-2 section of a manifest body. <see cref="Lines"/> holds non-empty content lines
/// (excluding HTML comments) paired with their 1-based line numbers.
/// </summary>
internal sealed record ManifestSection
{
    public required string Title { get; init; }

    public required int Line { get; init; }

    public required IReadOnlyList<(int Line, string Text)> Lines { get; init; }
}
