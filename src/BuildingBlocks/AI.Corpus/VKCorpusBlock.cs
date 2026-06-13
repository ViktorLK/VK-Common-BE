using VK.Blocks.Core;

namespace VK.Blocks.AI.Corpus;

/// <summary>
/// AI.Corpus Block Marker.
/// Follows BB.02.
/// </summary>
[VKBlockMarker(Dependencies = [typeof(VKAIBlock)])]
public sealed partial class VKCorpusBlock;
