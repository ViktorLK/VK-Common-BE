using VK.Blocks.Core;
using VK.Blocks.VectorStore;

namespace VK.Blocks.VectorIngest;

/// <summary>
/// Architectural marker for the AI Ingest building block.
/// </summary>
[VKBlockMarker(Dependencies = [typeof(VKVectorStoreBlock)])]
public sealed partial class VKVectorIngestBlock;
