using System;
using VK.Blocks.AI.Psyche;

namespace VK.Blocks.AI.Engram.Compression.Models;

/// <summary>
/// Metadata for the compression summary fragment.
/// </summary>
public sealed record VKCompressionSummaryMetadata : IVKFragmentMetadata
{
    public required string Summary { get; init; }
    public required VKChatSessionId SessionId { get; init; }
    public required int OriginalTokenCount { get; init; }
    public required int CompressedTokenCount { get; init; }
    public required DateTimeOffset CompressedAt { get; init; }
}
