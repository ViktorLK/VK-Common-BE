namespace VK.Blocks.AI.Eidos;

/// <summary>
/// Defines the chunk type for incremental streaming parsing.
/// </summary>
public enum VKStreamingChunkType : byte
{
    TextPart = 0,
    ToolPart = 1,
    DeltaPart = 2,
    ThinkingPart = 3
}
