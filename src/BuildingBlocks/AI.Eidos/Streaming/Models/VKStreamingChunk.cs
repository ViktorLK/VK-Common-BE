using System.Collections.Generic;

namespace VK.Blocks.AI.Eidos;

/// <summary>
/// Represents an incremental streaming chunk parsed by <see cref="IVKStreamingParser"/>.
/// Complies with [AP.01].
/// </summary>
public sealed record VKStreamingChunk // [AP.01]
{
    public VKStreamingChunkType ChunkType { get; init; } = VKStreamingChunkType.TextPart;
    public string DeltaText { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, object?> AvailableProperties { get; init; } = new Dictionary<string, object?>();
    public bool IsComplete { get; init; }
}
