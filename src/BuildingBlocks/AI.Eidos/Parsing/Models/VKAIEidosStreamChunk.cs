using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos;

public sealed record VKAIEidosStreamChunk
{
    public VKAIEidosChunkType ChunkType { get; init; } = VKAIEidosChunkType.TextPart;
    public string DeltaText { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, object?> AvailableProperties { get; init; } = new Dictionary<string, object?>();
    public bool IsComplete { get; init; }
}
