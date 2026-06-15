using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Corpus;

/// <summary>
/// AI.Corpus Block Marker.
/// Follows BB.02.
/// </summary>
[VKBlockMarker(Dependencies = [typeof(VKAIPsycheBlock)])]
public sealed partial class VKAICorpusBlock;
