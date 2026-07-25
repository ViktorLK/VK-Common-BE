using System;
using System.Collections.Generic;
using VK.Blocks.AI.Psyche;

namespace VK.Blocks.AI.Engram;

/// <summary>
/// Context for compression strategy execution.
/// </summary>
public sealed record VKCompressionContext
{
    /// <summary>
    /// Content string to compress.
    /// </summary>
    public required string Content { get; init; }

    /// <summary>
    /// Optional session ID associated with the compression context.
    /// </summary>
    public VKSessionId SessionId { get; init; }

    /// <summary>
    /// Existing L2 summary content if available.
    /// </summary>
    public string? ExistingL2Summary { get; init; }

    /// <summary>
    /// Source memory entries being compressed.
    /// </summary>
    public IReadOnlyList<VKMemoryEntry> SourceEntries { get; init; } = [];
}
