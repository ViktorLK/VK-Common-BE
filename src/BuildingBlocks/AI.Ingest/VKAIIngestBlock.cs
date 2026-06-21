using VK.Blocks.Core;

namespace VK.Blocks.AI.Ingest;

/// <summary>
/// Architectural marker for the AI Ingest building block.
/// </summary>
[VKBlockMarker(Dependencies = [typeof(VKAIBlock)])]
public sealed partial class VKAIIngestBlock;
