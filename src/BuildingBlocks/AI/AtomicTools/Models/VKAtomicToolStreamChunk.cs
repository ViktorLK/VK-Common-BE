namespace VK.Blocks.AI;

/// <summary>
/// Represents a single streaming event emitted by an atomic tool.
/// </summary>
public sealed record VKAtomicToolStreamChunk
{
    public VKAtomicToolStreamChunkKind Kind { get; init; } = VKAtomicToolStreamChunkKind.Text;

    public required string Content { get; init; }

    public double? ProgressPercentage { get; init; }
}

