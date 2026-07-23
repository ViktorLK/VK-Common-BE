using VK.Blocks.Core;

namespace VK.Blocks.VectorStore;

/// <summary>
/// Architectural marker for the AI Vector Store building block.
/// </summary>
[VKBlockMarker(Dependencies = [typeof(VKCoreBlock)], Toggleable = false)]
public sealed partial class VKVectorStoreBlock
{

    static partial void RegisterBlockCustom(IVKVectorStoreBuilder builder)
    {
    }

}
