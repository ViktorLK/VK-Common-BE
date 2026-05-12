using VK.Blocks.Core;

namespace VK.Blocks.AI.VectorStore;

/// <summary>
/// Architectural marker for the AI Vector Store building block.
/// </summary>
[VKBlockMarker(Dependencies = [typeof(VKCoreBlock)])]
public sealed partial class VKAIVectorStoreBlock;
